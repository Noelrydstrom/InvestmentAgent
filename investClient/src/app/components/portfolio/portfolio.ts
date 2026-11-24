import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { Subscription, interval, startWith, switchMap } from 'rxjs';
import { HoldingChart } from '../../holding-chart/holding-chart';
import { Holding } from '../../models/portfolio.models';
import { PortfolioService } from '../../services/portfolio.service';

@Component({
  selector: 'app-portfolio',
  imports: [DecimalPipe, HoldingChart],
  templateUrl: './portfolio.html',
  styleUrl: './portfolio.scss',
})
export class Portfolio implements OnInit, OnDestroy {
  private service = inject(PortfolioService);
  private subscription?: Subscription;

  holdings = signal<Holding[]>([]);
  cashBalance = signal(0);
  totalPortfolioValue = signal(0);
  totalAccountValue = signal(0);

  loading = signal(true);
  error = signal<string | null>(null);

  ngOnInit() {
    this.subscription = interval(5000)
      .pipe(
        startWith(0),
        switchMap(() => this.service.getPortfolio())
      )
      .subscribe({
        next: (data) => {
          this.holdings.set(data.holdings);
          this.cashBalance.set(data.cashBalance);
          this.totalPortfolioValue.set(data.totalPortfolioValue);
          this.totalAccountValue.set(data.totalAccountValue);

          this.loading.set(false);
          this.error.set(null);
        },
        error: () => {
          this.error.set('Kunde inte hämta portföljen. Kontrollera att API:et körs.');
          this.loading.set(false);
        },
      });
  }

  ngOnDestroy() {
    this.subscription?.unsubscribe();
  }
}