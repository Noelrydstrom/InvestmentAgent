namespace investAPI.Dtos;

public record PricePointDto(DateOnly Date, decimal Close);

public record HoldingDto(
    string Ticker,
    string Name,
    string Currency,
    int Shares,
    decimal AvgCostPrice,
    DateOnly PurchaseDate,
    decimal CurrentPrice,
    decimal ProfitLoss,       // totalt i bolagets valuta
    decimal ProfitLossPct,
    List<PricePointDto> Prices // kurspunkter från köpdatumet och framåt
);