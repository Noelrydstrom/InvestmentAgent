import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TradeProposal {
  orderId: string;
  transactionType: 'BUY' | 'SELL';
  ticker: string;
  companyName: string;
  shares: number;
  price: number;
  totalAmount: number;
  currency: string;
  createdAt: string;
  expiresAt: string;
}

export interface AiResponse {
  reply: string;
  tradeProposal: TradeProposal | null;
}

export interface TradeConfirmationResponse {
  message: string;
  transactionId: number;
  ticker: number;
  transactionType: string;
  shares: number;
  price: number;
  totalAmount: number;
  transactionDate: string;
}

@Injectable({ providedIn: 'root' })
export class AiService {
  private http = inject(HttpClient);

  private readonly apiUrl = 'http://localhost:5206/api/ai';
  private readonly tradingUrl = 'http://localhost:5206/api/trading';

  sendMessage(message: string): Observable<AiResponse> {
    return this.http.post<AiResponse>(
      `${this.apiUrl}/chat`,
      { message }
    );
  }

  confirmTrade(orderId: string): Observable<TradeConfirmationResponse> {
    return this.http.post<TradeConfirmationResponse>(
      `${this.tradingUrl}/confirm`,
      { orderId }
    );
  }
}