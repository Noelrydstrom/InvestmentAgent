import { Component, ElementRef, OnDestroy, afterNextRender, input, viewChild } from '@angular/core';
import { Chart, registerables } from 'chart.js';
import { Holding } from '../models/portfolio.models';

Chart.register(...registerables);

@Component({
  selector: 'app-holding-chart',
  template: `<div class="chart-box"><canvas #canvas></canvas></div>`,
  styles: [`.chart-box { position: relative; height: 220px; }`],
})
export class HoldingChart implements OnDestroy {
  holding = input.required<Holding>();
  private canvas = viewChild.required<ElementRef<HTMLCanvasElement>>('canvas');
  private chart?: Chart;

  constructor() {
    afterNextRender(() => this.draw());
  }

  private draw(): void {
    const h = this.holding();
    const color = h.currentPrice >= h.avgCostPrice ? '#16a34a' : '#dc2626';

    this.chart = new Chart(this.canvas().nativeElement, {
      type: 'line',
      data: {
        labels: h.prices.map((p) => p.date),
        datasets: [
          {
            label: `${h.ticker} kurs (${h.currency})`,
            data: h.prices.map((p) => p.close),
            borderColor: color,
            backgroundColor: color + '22',
            fill: true,
            tension: 0.25,
            pointRadius: 4,
          },
          {
            label: 'Inköpspris',
            data: h.prices.map(() => h.avgCostPrice),
            borderColor: '#6b7280',
            borderDash: [6, 6],
            pointRadius: 0,
            fill: false,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { position: 'bottom' } },
      },
    });
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
  }
}