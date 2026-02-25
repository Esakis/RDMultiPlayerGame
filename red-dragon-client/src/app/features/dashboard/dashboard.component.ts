import { Component, OnInit } from '@angular/core';
import { KingdomService } from '../../core/services/kingdom.service';
import { Kingdom } from '../../core/models/kingdom.model';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  kingdom: Kingdom | null = null;
  loading = true;
  message = '';

  constructor(private kingdomService: KingdomService) {}

  ngOnInit(): void {
    this.loadKingdom();
  }

  loadKingdom(): void {
    this.kingdomService.getMyKingdom().subscribe({
      next: (data) => {
        this.kingdom = data;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  useTurn(): void {
    if (this.kingdom && this.kingdom.turnsAvailable > 0) {
      this.kingdomService.useTurn().subscribe({
        next: (res) => {
          this.message = res.message || 'Tura wykorzystana.';
          this.loadKingdom();
          setTimeout(() => this.message = '', 3000);
        },
        error: (err) => {
          this.message = err.error?.message || 'Błąd.';
        }
      });
    }
  }
}
