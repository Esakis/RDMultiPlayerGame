import { Component, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { KingdomService } from '../../core/services/kingdom.service';
import { Kingdom } from '../../core/models/kingdom.model';

@Component({
  selector: 'app-economy',
  templateUrl: './economy.component.html',
  styleUrls: ['./economy.component.scss']
})
export class EconomyComponent implements OnInit {
  kingdom: Kingdom | null = null;
  loading = true;
  message = '';
  assignAmounts: { [key: string]: number } = {};

  // Pensja pracowników (0–50 złota/turę). Steruje popularnością: cel = pensja × 2 (maks. 100%).
  wagesInput = 50;

  constructor(private kingdomService: KingdomService, private translate: TranslateService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.kingdomService.getMyKingdom().subscribe({
      next: (k) => { this.kingdom = k; this.wagesInput = k.wages; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  /** Docelowa popularność dla podanej pensji (cel = pensja × 2, maks. 100%). */
  get wageTargetPopularity(): number {
    return Math.min(100, (this.wagesInput || 0) * 2);
  }

  /** Liczba bezrobotnych (pula, z której przydzielamy do zawodów). */
  get unemployed(): number {
    return this.kingdom?.professions?.find(p => p.professionType === 'Bezrobotni')?.workerCount ?? 0;
  }

  /** Zawody, do których można przydzielać pracowników (bez puli „Bezrobotni"). */
  get assignableProfessions() {
    return this.kingdom?.professions?.filter(p => p.professionType !== 'Bezrobotni') ?? [];
  }

  setWages(): void {
    this.kingdomService.setWages(this.wagesInput).subscribe({
      next: (res) => { this.message = res.message || 'OK'; this.load(); this.clearMsg(); },
      error: (err) => { this.message = err.error?.message || err.error || 'Błąd'; this.clearMsg(); }
    });
  }

  assign(profType: string, amount: number): void {
    this.kingdomService.assignWorkers({ professionType: profType, workerCount: amount }).subscribe({
      next: (res) => { this.message = res.message || 'OK'; this.load(); this.clearMsg(); },
      error: (err) => { this.message = err.error?.message || err.error || 'Błąd'; this.clearMsg(); }
    });
  }

  unassign(profType: string, amount: number): void {
    this.assign(profType, -amount);
  }

  getProductionInfo(profType: string): string {
    const key = `eco.prod.${profType}`;
    const t = this.translate.instant(key);
    return t === key ? '-' : t;
  }

  private clearMsg(): void { setTimeout(() => this.message = '', 4000); }
}
