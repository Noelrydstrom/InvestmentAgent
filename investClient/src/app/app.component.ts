import { Component, inject, signal } from '@angular/core';

import { Portfolio } from './components/portfolio/portfolio';
import { HudTopBarComponent } from './components/hud-top-bar/hud-top-bar';
import { AiCoreComponent, type AiCoreStatus } from './features/ai-core/ai-core';
import { NewsFeedComponent } from './features/news-feed/news-feed';
import { AiService } from './services/ai.service';

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

handleCommand(command: string): void {

this.aiStatus.set('thinking');

this.aiService.sendMessage(command).subscribe({

  next: (response) => {
    this.aiResponse.set(response.reply);
    this.aiStatus.set('idle');
  },

  error: (error) => {
    console.error('AI API Error:', error);

    this.aiResponse.set('Unable to connect to InvestAI. Please try again.');
    this.aiStatus.set('idle');
  }
});

}
}
