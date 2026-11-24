namespace investAPI.Dtos;

public class TradeRequestDto
{
    public string Ticker { get; set; } = null!;

    // BUY eller SELL
    public string TransactionType { get; set; } = null!;

    // Belopp i SEK som användaren vill köpa eller sälja för
    public decimal Amount { get; set; }
}