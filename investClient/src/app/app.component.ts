import { Component } from '@angular/core';
import { Portfolio } from './components/portfolio/portfolio';

@Component({
  selector: 'app-root',
  imports: [Portfolio],
  templateUrl: './app.component.html',
  styleUrl: './app.scss',
})
export class App {}
