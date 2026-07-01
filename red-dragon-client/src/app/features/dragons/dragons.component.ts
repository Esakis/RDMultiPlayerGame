import { Component, OnInit } from '@angular/core';
import { DragonService, DragonStatus } from '../../core/services/dragon.service';

@Component({
  selector: 'app-dragons',
  templateUrl: './dragons.component.html',
  styleUrls: ['./dragons.component.scss']
})
export class DragonsComponent implements OnInit {
  status: DragonStatus | null = null;
  loading = true;
  error = '';
  message = '';
  summoning = false;

  constructor(private dragon: DragonService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.dragon.getStatus().subscribe({
      next: s => { this.status = s; this.loading = false; },
      error: () => { this.loading = false; this.error = 'Błąd wczytywania smoków.'; }
    });
  }

  get fillPct(): number {
    if (!this.status || this.status.cap <= 0) return 0;
    return Math.min(100, Math.round(this.status.dragons / this.status.cap * 100));
  }

  summon(): void {
    this.message = '';
    this.error = '';
    this.summoning = true;
    this.dragon.summon().subscribe({
      next: r => { this.message = r.message || ''; this.summoning = false; this.load(); },
      error: e => { this.error = e.error?.message || e.error || 'Błąd przywołania.'; this.summoning = false; }
    });
  }
}
