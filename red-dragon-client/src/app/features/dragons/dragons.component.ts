import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
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

  constructor(private dragon: DragonService, private router: Router) {}

  ngOnInit(): void {
    this.dragon.getStatus().subscribe({
      next: s => { this.status = s; this.loading = false; },
      error: () => { this.loading = false; this.error = 'Błąd wczytywania smoków.'; }
    });
  }

  get fillPct(): number {
    if (!this.status || this.status.cap <= 0) return 0;
    return Math.min(100, Math.round(this.status.dragons / this.status.cap * 100));
  }

  goToMagic(): void {
    this.router.navigate(['/magic']);
  }
}
