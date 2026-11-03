using investAPI.Data;
using investAPI.Dtos;
using Microsoft.EntityFrameworkCore;

namespace investAPI.Services;

public interface IPortfolioService
{
    Task<List<HoldingDto>> GetPortfolioAsync(int userId);
    Task<List<PricePointDto>?> GetPricesAsync(string ticker, DateOnly? from = null);
}

public class PortfolioService(InvestmentDbContext db) : IPortfolioService
{
    public async Task<List<HoldingDto>> GetPortfolioAsync(int userId)
    {
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
            .Select(p => new { p.SecurityId, p.PriceDate, p.ClosePrice })
            .ToListAsync();

        return holdings
            .OrderBy(h => h.Ticker)
            .Select(h =>
            {
                var own = allPrices.Where(p => p.SecurityId == h.SecurityId).ToList();
                var current = own.Count > 0 ? own[^1].ClosePrice : h.AvgCostPrice;

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
                    Math.Round((current / h.AvgCostPrice - 1) * 100, 2),
                    sincePurchase);
            })
            .ToList();
    }

    public async Task<List<PricePointDto>?> GetPricesAsync(string ticker, DateOnly? from = null)
    {
        var securityId = await db.Securities
            .Where(s => s.Ticker == ticker)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync();

        if (securityId is null) return null;

        var query = db.PriceHistories.Where(p => p.SecurityId == securityId);

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