import { Component, inject, signal } from '@angular/core';

import { Portfolio } from './components/portfolio/portfolio';
import { HudTopBarComponent } from './components/hud-top-bar/hud-top-bar';
import { AiCoreComponent, type AiCoreStatus } from './features/ai-core/ai-core';
import { NewsFeedComponent } from './features/news-feed/news-feed';
import { AiService, type TradeProposal } from './services/ai.service';

@Component({
  selector: 'app-root',
  imports: [
    Portfolio,
    HudTopBarComponent,
    AiCoreComponent,
    NewsFeedComponent
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.scss',
})
export class App {

  private aiService = inject(AiService);

  aiStatus = signal<AiCoreStatus>('idle');

  aiResponse = signal(
    'System online. InvestAI is ready and waiting for your instructions.'
  );

  tradeProposal = signal<TradeProposal | null>(null);

  handleCommand(command: string): void {
    this.aiStatus.set('thinking');
    this.tradeProposal.set(null);

    this.aiService.sendMessage(command).subscribe({

      next: (response) => {
        this.aiResponse.set(response.reply);
        this.tradeProposal.set(response.tradeProposal);
        this.aiStatus.set('idle');
      },

      error: (error) => {
        console.error('AI API Error:', error);

        this.aiResponse.set(
          error.error?.message ??
          'Unable to connect to InvestAI. Please try again.'
        );

        this.aiStatus.set('idle');
      }
    });
  }

  confirmTrade(orderId: string): void {
    this.aiStatus.set('executing');

    this.aiService.confirmTrade(orderId).subscribe({

      next: (response) => {
        this.aiResponse.set(
          `${response.message} ${response.transactionType} genomförd.`
        );

        this.tradeProposal.set(null);
        this.aiStatus.set('idle');
      },

      error: (error) => {
        console.error('Trade confirmation error:', error);

        this.aiResponse.set(
          error.error?.message ??
          'Kunde inte genomföra affären. Försök igen.'
        );

        this.tradeProposal.set(null);
        this.aiStatus.set('idle');
      }
    });
  }
}