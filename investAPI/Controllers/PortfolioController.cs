using investAPI.Dtos;
using investAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace investAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortfolioController(IPortfolioService portfolio) : ControllerBase
{
    private const int DemoUserId = 1; // ingen inloggning än, demoanvändaren

    [HttpGet]
    public async Task<ActionResult<List<HoldingDto>>> Get()
        => Ok(await portfolio.GetPortfolioAsync(DemoUserId));
}

[ApiController]
[Route("api/[controller]")]
public class SecuritiesController(IPortfolioService portfolio) : ControllerBase
{
    [HttpGet("{ticker}/prices")]
    public async Task<ActionResult<List<PricePointDto>>> GetPrices(string ticker, [FromQuery] DateOnly? from)
    {
        var prices = await portfolio.GetPricesAsync(ticker, from);
        return prices is null ? NotFound($"Okänd ticker: {ticker}") : Ok(prices);
    }
}