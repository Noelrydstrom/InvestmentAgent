using investAPI.Data;
using investAPI.Dtos;
using Microsoft.EntityFrameworkCore;

namespace investAPI.Services;

public interface IPortfolioService
{
    Task<PortfolioOverviewDto> GetPortfolioAsync(int userId);
    Task<List<PricePointDto>?> GetPricesAsync(string ticker, DateOnly? from = null);
}

public class PortfolioService(InvestmentDbContext db) : IPortfolioService
{
    public async Task<PortfolioOverviewDto> GetPortfolioAsync(int userId)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new InvalidOperationException("Användaren kunde inte hittas.");

        var holdings = await db.Holdings
            .Where(h => h.UserId == userId)
            .Select(h => new
            {
                h.SecurityId,
                h.Shares,
                h.AvgCostPrice,
                h.PurchaseDate,
                h.Security.Ticker,
                h.Security.Name,
                h.Security.Currency
            })
            .ToListAsync();

        var securityIds = holdings.Select(h => h.SecurityId).ToList();

        var allPrices = await db.PriceHistories
            .Where(p => securityIds.Contains(p.SecurityId))
            .OrderBy(p => p.PriceDate)
            .Select(p => new
            {
                p.SecurityId,
                p.PriceDate,
                p.ClosePrice
            })
            .ToListAsync();

        var portfolioOverview = new PortfolioOverviewDto();

        portfolioOverview.Holdings = holdings
            .OrderBy(h => h.Ticker)
            .Select(h =>
            {
                var own = allPrices
                    .Where(p => p.SecurityId == h.SecurityId)
                    .ToList();

                var current = own.Count > 0
                    ? own[^1].ClosePrice
                    : h.AvgCostPrice;

                var sincePurchase = own
                    .Where(p => p.PriceDate >= h.PurchaseDate)
                    .Select(p => new PricePointDto(p.PriceDate, p.ClosePrice))
                    .ToList();

                return new HoldingDto(
                    h.Ticker,
                    h.Name,
                    h.Currency,
                    h.Shares,
                    h.AvgCostPrice,
                    h.PurchaseDate,
                    current,
                    Math.Round((current - h.AvgCostPrice) * h.Shares, 2),
                    h.AvgCostPrice > 0
                        ? Math.Round((current / h.AvgCostPrice - 1) * 100, 2)
                        : 0,
                    sincePurchase
                );
            })
            .ToList();

        portfolioOverview.CashBalance = user.CashBalance;

        portfolioOverview.TotalPortfolioValue = Math.Round(
            portfolioOverview.Holdings.Sum(h => h.Shares * h.CurrentPrice), 2
        );

        portfolioOverview.TotalAccountValue = Math.Round(
            portfolioOverview.TotalPortfolioValue + portfolioOverview.CashBalance, 2
        );

        return portfolioOverview;
    }

    public async Task<List<PricePointDto>?> GetPricesAsync(
        string ticker,
        DateOnly? from = null)
    {
        var securityId = await db.Securities
            .Where(s => s.Ticker == ticker)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync();

        if (securityId is null)
            return null;

        var query = db.PriceHistories
            .Where(p => p.SecurityId == securityId);

        if (from is not null)
        {
            var start = from.Value;
            query = query.Where(p => p.PriceDate >= start);
        }

        return await query
            .OrderBy(p => p.PriceDate)
            .Select(p => new PricePointDto(p.PriceDate, p.ClosePrice))
            .ToListAsync();
    }
}