import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Holding, PricePoint } from '../models/portfolio.models';

@Injectable({ providedIn: 'root' })
export class PortfolioService {
  private http = inject(HttpClient);
  private baseUrl = 'https://localhost:7237/api';

  getPortfolio(): Observable<Holding[]> {
    return this.http.get<Holding[]>(`${this.baseUrl}/portfolio`);
  }

  getPrices(ticker: string): Observable<PricePoint[]> {
    return this.http.get<PricePoint[]>(`${this.baseUrl}/securities/${ticker}/prices`);
  }
}