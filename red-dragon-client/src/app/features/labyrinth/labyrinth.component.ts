import { Component, OnInit } from '@angular/core';
import { LabyrinthService, LabyrinthStatus, LabyrinthResult } from '../../core/services/labyrinth.service';
import { Observable } from 'rxjs';

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

  private handle(obs: Observable<LabyrinthResult>): void {
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

  takeTreasure(type: string): void {
    if (!this.selectedGeneralId) { this.error = 'Wybierz generała.'; return; }
    this.handle(this.labyrinth.takeTreasure(this.selectedGeneralId, type));
  }

  searchGeneral(): void {
    if (!this.selectedGeneralId) { this.error = 'Wybierz generała.'; return; }
    this.handle(this.labyrinth.searchGeneral(this.selectedGeneralId));
  }

  changeAbility(): void {
    if (!this.selectedGeneralId) { this.error = 'Wybierz generała.'; return; }
    this.handle(this.labyrinth.changeAbility(this.selectedGeneralId));
  }
}
