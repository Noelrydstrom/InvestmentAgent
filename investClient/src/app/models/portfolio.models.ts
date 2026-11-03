export interface PricePoint {
  date: string;
  close: number;
}

export interface Holding {
  ticker: string;
  name: string;
  currency: string;
  shares: number;
  avgCostPrice: number;
  purchaseDate: string;
  currentPrice: number;
  profitLoss: number;
  profitLossPct: number;
  prices: PricePoint[];
}