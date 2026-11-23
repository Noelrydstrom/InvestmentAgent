using investAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace investAPI.Services;

public interface IInvestmentDataService
{
    Task<string> GetMarketOverviewAsync(CancellationToken cancellationToken);
}

public class InvestmentDataService(
    InvestmentDbContext dbContext) : IInvestmentDataService
{
    public async Task<string> GetMarketOverviewAsync(
        CancellationToken cancellationToken)
    {
        var securities = await dbContext.Securities
            .AsNoTracking()
            .Select(security => new
            {
                security.Name,
                security.Ticker,
                security.Sector,
                security.Currency,

                LatestPrice = security.PriceHistories
                    .OrderByDescending(price => price.PriceDate)
                    .Select(price => new
                    {
                        price.ClosePrice,
                        price.PriceDate
                    })
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        if (securities.Count == 0)
        {
            return "There are currently no securities in the database.";
        }

        var result = new System.Text.StringBuilder();

        result.AppendLine("MARKET DATA FROM INVESTMENTDB");
        result.AppendLine("The following data comes directly from the local database.");
        result.AppendLine();

        foreach (var security in securities)
        {
            result.AppendLine($"Company: {security.Name}");
            result.AppendLine($"Ticker: {security.Ticker}");
            result.AppendLine($"Sector: {security.Sector ?? "Unknown"}");

            if (security.LatestPrice != null)
            {
                result.AppendLine(
                    $"Latest price: {security.LatestPrice.ClosePrice} {security.Currency}");

                result.AppendLine(
                    $"Price date: {security.LatestPrice.PriceDate}");
            }
            else
            {
                result.AppendLine("Latest price: No price data available.");
            }

            result.AppendLine();
        }

        return result.ToString();
    }
}