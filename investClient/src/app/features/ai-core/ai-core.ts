import { Component, EventEmitter, Input, Output, signal } from '@angular/core';

export type AiCoreStatus =
  | 'idle'
  | 'thinking'
  | 'analyzing'
  | 'executing';

@Component({
  selector: 'app-ai-core',
  standalone: true,
  imports: [],
  templateUrl: './ai-core.html',
  styleUrl: './ai-core.scss'
})
export class AiCoreComponent {

  @Input() status: AiCoreStatus = 'idle';
  @Input() set response(value: string) {
    this.responseText.set(value);
  }

  @Output() commandSubmitted = new EventEmitter<string>();

  // InvestAI response
  responseText = signal(
    'System online. InvestAI is ready and waiting for your instructions.'
  );

  responseConfidence = signal('87%');
  responseRisk = signal('Low');
  responseRecommendation = signal('Analyzing');

  command = '';

  suggestions = [
    'Analyze my portfolio',
    'Find undervalued stocks',
    'Compare current holdings',
    'Identify biggest risks'
  ];

  get statusLabel(): string {
    switch (this.status) {
      case 'thinking':
        return 'PROCESSING REQUEST';

      case 'analyzing':
        return 'ANALYZING INVESTMENTS';

      case 'executing':
        return 'EXECUTING COMMAND';

      default:
        return 'SYSTEM ONLINE';
    }
  }

  updateCommand(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.command = input.value;
  }

  useSuggestion(suggestion: string): void {
    this.command = suggestion;
  }

  executeCommand(event: Event): void {
    event.preventDefault();

    const instruction = this.command.trim();

    if (!instruction || this.status === 'executing') {
      return;
    }

    this.commandSubmitted.emit(instruction);

    this.command = '';
  }
}