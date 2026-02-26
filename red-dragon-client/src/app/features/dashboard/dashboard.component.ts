import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { KingdomService } from '../../core/services/kingdom.service';
import { TurnService } from '../../core/services/turn.service';
import { Kingdom } from '../../core/models/kingdom.model';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit, OnDestroy {
  kingdom: Kingdom | null = null;
  loading = true;
  message = '';
  deltas: { [key: string]: number } = {};
  private turnSub!: Subscription;

  constructor(
    private kingdomService: KingdomService,
    private turnService: TurnService
  ) {}

  ngOnInit(): void {
    this.loadKingdom();
    this.turnSub = this.turnService.turnProcessed$.subscribe(deltas => {
      this.deltas = deltas;
      this.loadKingdom();
    });
  }

  ngOnDestroy(): void {
    if (this.turnSub) this.turnSub.unsubscribe();
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
          this.deltas = res.deltas || {};
          this.turnService.emitDeltas(this.deltas);
          this.loadKingdom();
          setTimeout(() => this.message = '', 5000);
        },
        error: (err) => {
          this.message = err.error?.message || 'Błąd.';
        }
      });
    }
  }

  getDelta(key: string): string {
    const val = this.deltas[key];
    if (val === undefined || val === 0) return '';
    return val > 0 ? `+${val.toLocaleString('pl-PL')}` : val.toLocaleString('pl-PL');
  }

  getDeltaClass(key: string): string {
    const val = this.deltas[key];
    if (val === undefined || val === 0) return '';
    return val > 0 ? 'positive' : 'negative';
  }
}
