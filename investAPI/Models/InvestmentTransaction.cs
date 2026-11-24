namespace investAPI.Models;

public class InvestmentTransaction
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int SecurityId { get; set; }

    public string TransactionType { get; set; } = null!;

    public decimal Shares { get; set; }

    public decimal Price { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime TransactionDate { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual Security Security { get; set; } = null!;
}