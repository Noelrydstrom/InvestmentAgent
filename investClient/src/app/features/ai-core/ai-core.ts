import { Component, EventEmitter, Input, Output } from '@angular/core';

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

  @Output() commandSubmitted = new EventEmitter<string>();

  command = '';

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