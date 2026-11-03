import { Component, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { HoldingChart } from '../../holding-chart/holding-chart';
import { Holding } from '../../models/portfolio.models';
import { PortfolioService } from '../../services/portfolio.service';

@Component({
  selector: 'app-portfolio',
  imports: [DecimalPipe, HoldingChart],
  templateUrl: './portfolio.html',
  styleUrl: './portfolio.scss',
})
export class Portfolio {
  private service = inject(PortfolioService);

  holdings = signal<Holding[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  constructor() {
    this.service.getPortfolio().subscribe({
      next: (data) => {
        this.holdings.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Kunde inte hämta portföljen. Kontrollera att API:et körs.');
        this.loading.set(false);
      },
    });
  }
}