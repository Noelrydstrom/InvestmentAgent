using investAPI.Data;
using investAPI.Dtos;
using investAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace investAPI.Services;

public interface ITradingService
{
    Task<TradeProposalDto> CreateTradeProposalAsync(int userId, TradeRequestDto request);
    Task<InvestmentTransaction> ConfirmTradeAsync(int userId, TradeConfirmationDto request);
}

public class TradingService(InvestmentDbContext db) : ITradingService
{
    public async Task<TradeProposalDto> CreateTradeProposalAsync(
    int userId,
    TradeRequestDto request)
    {
        // 1. Validera förfrågan
        if (request.Amount <= 0)
            throw new InvalidOperationException("Beloppet måste vara större än noll.");

        var transactionType = request.TransactionType.Trim().ToUpperInvariant();

        if (transactionType != "BUY" && transactionType != "SELL")
            throw new InvalidOperationException("Ogiltig transaktionstyp.");

        var ticker = request.Ticker.Trim().ToUpperInvariant();

        // 2. Hämta bolaget
        var security = await db.Securities
            .FirstOrDefaultAsync(s => s.Ticker == ticker);

        if (security == null)
            throw new InvalidOperationException("Bolaget kunde inte hittas.");

        // Vi börjar med att endast stödja SEK
        if (security.Currency != "SEK")
            throw new InvalidOperationException("Handel stöds för närvarande endast i SEK.");

        // 3. Hämta senaste registrerade aktiekurs
        var latestPrice = await db.PriceHistories
            .Where(p => p.SecurityId == security.Id)
            .OrderByDescending(p => p.PriceDate)
            .Select(p => (decimal?)p.ClosePrice)
            .FirstOrDefaultAsync();

        if (latestPrice == null || latestPrice <= 0)
            throw new InvalidOperationException("Det finns ingen giltig aktiekurs för bolaget.");

        var price = latestPrice.Value;

        // 4. Beräkna antal aktier
        // Vi avrundar nedåt till sex decimaler för att inte överskrida beloppet.
        var shares = Math.Floor((request.Amount / price) * 1_000_000m)
                     / 1_000_000m;

        if (shares <= 0)
            throw new InvalidOperationException("Beloppet är för litet för att köpa eller sälja aktier.");

        var totalAmount = Math.Round(shares * price, 2);

        // 5. Kontrollera att användaren finns
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new InvalidOperationException("Användaren kunde inte hittas.");

        // 6. Kontrollera saldo eller innehav
        if (transactionType == "BUY")
        {
            if (user.CashBalance < totalAmount)
                throw new InvalidOperationException("Du har inte tillräckligt med pengar på kontot.");
        }
        else
        {
            var holding = await db.Holdings
                .FirstOrDefaultAsync(h =>
                    h.UserId == userId &&
                    h.SecurityId == security.Id);

            if (holding == null || holding.Shares < shares)
                throw new InvalidOperationException("Du äger inte tillräckligt många aktier för försäljningen.");
        }

        // 7. Skapa väntande order
        var now = DateTime.UtcNow;

        var pendingTrade = new PendingTrade
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SecurityId = security.Id,
            TransactionType = transactionType,
            Shares = shares,
            Price = price,
            TotalAmount = totalAmount,
            Status = "Pending",
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(5)
        };

        db.PendingTrades.Add(pendingTrade);

        await db.SaveChangesAsync();

        // 8. Skicka tillbaka orderförslaget
        return new TradeProposalDto
        {
            OrderId = pendingTrade.Id,
            TransactionType = transactionType,
            Ticker = security.Ticker,
            CompanyName = security.Name,
            Shares = shares,
            Price = price,
            TotalAmount = totalAmount,
            Currency = security.Currency,
            CreatedAt = now,
            ExpiresAt = pendingTrade.ExpiresAt
        };
    }

    public async Task<InvestmentTransaction> ConfirmTradeAsync(
        int userId,
        TradeConfirmationDto request)
    {
        // Använd Serializable för att skydda mot samtidiga bekräftelser.
        await using var dbTransaction = await db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable);

        // 1. Hämta ordern
        var pendingTrade = await db.PendingTrades
            .FirstOrDefaultAsync(p =>
                p.Id == request.OrderId &&
                p.UserId == userId);

        if (pendingTrade == null)
            throw new InvalidOperationException("Ordern kunde inte hittas.");

        // 2. Kontrollera orderstatus
        if (pendingTrade.Status != "Pending")
            throw new InvalidOperationException("Ordern är redan behandlad.");

        // 3. Kontrollera giltighetstid
        if (pendingTrade.ExpiresAt <= DateTime.UtcNow)
            throw new InvalidOperationException("Ordern har gått ut. Skapa ett nytt orderförslag.");

        // 4. Hämta användare och bolag
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new InvalidOperationException("Användaren kunde inte hittas.");

        var security = await db.Securities
            .FirstOrDefaultAsync(s => s.Id == pendingTrade.SecurityId);

        if (security == null)
            throw new InvalidOperationException("Bolaget kunde inte hittas.");

        // 5. Kontrollera att aktiekursen inte har ändrats
        var latestPrice = await db.PriceHistories
            .Where(p => p.SecurityId == security.Id)
            .OrderByDescending(p => p.PriceDate)
            .Select(p => (decimal?)p.ClosePrice)
            .FirstOrDefaultAsync();

        if (latestPrice == null || latestPrice.Value != pendingTrade.Price)
            throw new InvalidOperationException(
                "Aktiekursen har ändrats. Skapa ett nytt orderförslag.");

        // 6. Genomför köpet eller försäljningen
        if (pendingTrade.TransactionType == "BUY")
        {
            // Kontrollera saldo igen
            if (user.CashBalance < pendingTrade.TotalAmount)
                throw new InvalidOperationException(
                    "Du har inte tillräckligt med pengar på kontot.");

            var holding = await db.Holdings
                .FirstOrDefaultAsync(h =>
                    h.UserId == userId &&
                    h.SecurityId == security.Id);

            if (holding == null)
            {
                // Skapa nytt innehav
                holding = new Holding
                {
                    UserId = userId,
                    SecurityId = security.Id,
                    Shares = pendingTrade.Shares,
                    AvgCostPrice = pendingTrade.Price,
                    PurchaseDate = DateOnly.FromDateTime(DateTime.UtcNow)
                };

                db.Holdings.Add(holding);
            }
            else
            {
                // Uppdatera befintligt innehav
                var totalShares = holding.Shares + pendingTrade.Shares;

                var totalCost =
                    (holding.Shares * holding.AvgCostPrice) +
                    (pendingTrade.Shares * pendingTrade.Price);

                holding.AvgCostPrice = Math.Round(
                    totalCost / totalShares, 2);

                holding.Shares = totalShares;
            }

            user.CashBalance -= pendingTrade.TotalAmount;
        }
        else if (pendingTrade.TransactionType == "SELL")
        {
            var holding = await db.Holdings
                .FirstOrDefaultAsync(h =>
                    h.UserId == userId &&
                    h.SecurityId == security.Id);

            if (holding == null || holding.Shares < pendingTrade.Shares)
                throw new InvalidOperationException(
                    "Du äger inte tillräckligt många aktier för försäljningen.");

            // Minska innehavet
            holding.Shares -= pendingTrade.Shares;

            // Öka kontosaldot
            user.CashBalance += pendingTrade.TotalAmount;

            // Ta bort innehavet om användaren inte längre äger några aktier
            if (holding.Shares == 0)
            {
                db.Holdings.Remove(holding);
            }
        }
        else
        {
            throw new InvalidOperationException("Ogiltig transaktionstyp.");
        }

        // 7. Registrera genomförd transaktion
        var investmentTransaction = new InvestmentTransaction
        {
            UserId = userId,
            SecurityId = security.Id,
            TransactionType = pendingTrade.TransactionType,
            Shares = pendingTrade.Shares,
            Price = pendingTrade.Price,
            TotalAmount = pendingTrade.TotalAmount,
            TransactionDate = DateTime.UtcNow
        };

        db.InvestmentTransactions.Add(investmentTransaction);

        // 8. Markera ordern som genomförd
        pendingTrade.Status = "Executed";

        // 9. Spara alla ändringar
        await db.SaveChangesAsync();

        await dbTransaction.CommitAsync();

        return investmentTransaction;
    }

}
