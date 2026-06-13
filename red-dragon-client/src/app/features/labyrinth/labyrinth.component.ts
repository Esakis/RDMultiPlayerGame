import { Component, OnInit } from '@angular/core';
import { LabyrinthService, LabyrinthStatus, LabyrinthResult } from '../../core/services/labyrinth.service';

@Component({
  selector: 'app-labyrinth',
  templateUrl: './labyrinth.component.html',
  styleUrls: ['./labyrinth.component.scss']
})
export class LabyrinthComponent implements OnInit {
  status: LabyrinthStatus | null = null;
  selectedGeneralId?: number;
  message = '';
  error = '';
  loading = true;
  busy = false;

  constructor(private labyrinth: LabyrinthService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.labyrinth.getStatus().subscribe({
      next: s => {
        this.status = s;
        this.loading = false;
        if (!this.selectedGeneralId && s.availableGenerals.length > 0) {
          this.selectedGeneralId = s.availableGenerals[0].id;
        }
      },
      error: () => { this.loading = false; this.error = 'Błąd wczytywania labiryntu.'; }
    });
  }

  private handle(obs: ReturnType<LabyrinthService['advance']>): void {
    this.busy = true;
    this.message = '';
    this.error = '';
    obs.subscribe({
      next: (r: LabyrinthResult) => {
        this.message = r.message ?? '';
        if (r.data) this.status = r.data;
        this.busy = false;
      },
      error: e => { this.error = e.error || 'Błąd akcji w labiryncie.'; this.busy = false; this.load(); }
    });
  }

  enter(): void {
    if (!this.selectedGeneralId) { this.error = 'Wybierz generała.'; return; }
    this.handle(this.labyrinth.enter(this.selectedGeneralId));
  }

  advance(): void {
    this.handle(this.labyrinth.advance());
  }

  retreat(): void {
    this.handle(this.labyrinth.retreat());
  }

  spendDice(type: string): void {
    this.handle(this.labyrinth.spend(type));
  }
}
