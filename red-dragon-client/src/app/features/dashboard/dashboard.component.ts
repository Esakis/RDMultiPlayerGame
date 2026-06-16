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

  races = ['Człowiek', 'Elf', 'Krasnolud', 'Hobbit', 'Nekromant', 'Dżin', 'Goblin', 'Ent', 'Olbrzym'];
  selectedRace = '';

  get constructionEvents() {
    return this.kingdom?.recentEvents?.filter(e => e.category === 'Construction') ?? [];
  }

  get trainingEvents() {
    return this.kingdom?.recentEvents?.filter(e => e.category === 'Training') ?? [];
  }

  get hasAnyEvent(): boolean {
    return !!this.kingdom &&
      (this.kingdom.pendingGeneralCount > 0 ||
       (this.kingdom.activeSpells?.length ?? 0) > 0 ||
       (this.kingdom.recentEvents?.length ?? 0) > 0);
  }

  /** Numer bieżącej tury (1-indeksowany): nowy dzień startuje od 1, ostatnia = przydział. */
  get currentTurnNumber(): number {
    if (!this.kingdom) return 0;
    const used = this.kingdom.turnsCapacity - this.kingdom.turnsAvailable;
    return this.kingdom.turnsAvailable > 0
      ? Math.min(used + 1, this.kingdom.turnsCapacity)
      : this.kingdom.turnsCapacity;
  }

  get canChangeRace(): boolean {
    return !!this.kingdom?.buildings?.some(b => b.buildingType === 'PalacZmian' && b.quantity > 0 && !b.isUnderConstruction);
  }

  changeRace(): void {
    if (!this.selectedRace) { this.message = 'Wybierz rasę.'; return; }
    this.kingdomService.changeRace(this.selectedRace).subscribe({
      next: (res) => { this.message = res.message || ''; this.selectedRace = ''; this.loadKingdom(); setTimeout(() => this.message = '', 6000); },
      error: (err) => { this.message = err.error?.message || err.error || 'Błąd.'; setTimeout(() => this.message = '', 6000); }
    });
  }

  toggleFreeze(): void {
    if (!this.kingdom) return;
    const op = this.kingdom.isFrozen
      ? this.kingdomService.unfreeze()
      : this.kingdomService.freeze();
    op.subscribe({
      next: (res) => { this.message = res.message || ''; this.loadKingdom(); setTimeout(() => this.message = '', 5000); },
      error: (err) => { this.message = err.error?.message || err.error || 'Błąd.'; setTimeout(() => this.message = '', 5000); }
    });
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
