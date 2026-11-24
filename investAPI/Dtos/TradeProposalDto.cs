namespace investAPI.Dtos;

public class TradeProposalDto
{
    public Guid OrderId { get; set; }

    public string TransactionType { get; set; } = null!;

    public string Ticker { get; set; } = null!;
    public string CompanyName { get; set; } = null!;

    public decimal Shares { get; set; }
    public decimal Price { get; set; }
    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}