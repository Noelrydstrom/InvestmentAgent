import { Component } from '@angular/core';
import { Portfolio } from './components/portfolio/portfolio';
import { HudTopBarComponent } from './components/hud-top-bar/hud-top-bar';

@Component({
  selector: 'app-root',
  imports: [Portfolio, HudTopBarComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.scss',
})
export class App {}
