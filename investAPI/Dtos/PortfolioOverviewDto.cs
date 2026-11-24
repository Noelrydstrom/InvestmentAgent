namespace investAPI.Dtos;

public class PortfolioOverviewDto
{
    public List<HoldingDto> Holdings { get; set; } = new();
    public decimal CashBalance { get; set; }
    public decimal TotalPortfolioValue { get; set; }
    public decimal TotalAccountValue { get; set; }
}