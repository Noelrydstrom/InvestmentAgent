import { Component } from '@angular/core';
import { Portfolio } from './components/portfolio/portfolio';
import { HudTopBarComponent } from './components/hud-top-bar/hud-top-bar';
import { AiCoreComponent, type AiCoreStatus } from './features/ai-core/ai-core';

@Component({
  selector: 'app-root',
  imports: [
    Portfolio,
    HudTopBarComponent,
    AiCoreComponent
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.scss',
})
export class App {

  aiStatus: AiCoreStatus = 'idle';

  handleCommand(command: string): void {
    console.log('AI Command:', command);

    this.aiStatus = 'thinking';
  }

}