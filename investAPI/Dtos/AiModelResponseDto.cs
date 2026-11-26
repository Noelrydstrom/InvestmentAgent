namespace investAPI.Dtos;

public class AiModelResponseDto
{
    public string Reply { get; set; } = "";

    public bool IsTradeRequest { get; set; }

    public string? TransactionType { get; set; }

    public string? Ticker { get; set; }

    public decimal? Amount { get; set; }
}