import { Component, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { MilitaryService } from '../../core/services/military.service';
import { KingdomService } from '../../core/services/kingdom.service';
import { CoalitionService, War } from '../../core/services/coalition.service';
import { KingdomSummary, Kingdom, MilitaryUnit } from '../../core/models/kingdom.model';

interface TargetRow {
  k: KingdomSummary;
  attackable: boolean;
  reason: string;
}

@Component({
  selector: 'app-attack',
  templateUrl: './attack.component.html',
  styleUrls: ['./attack.component.scss']
})
export class AttackComponent implements OnInit {
  myKingdom: Kingdom | null = null;
  myUnits: MilitaryUnit[] = [];
  allKingdoms: KingdomSummary[] = [];
  enemyCoalitionIds = new Set<number>();
  enemyCoalitionNames: string[] = [];

  filterMode: 'all' | 'attackable' = 'attackable';
  coalitionFilter = 'all';

  selectedTarget: KingdomSummary | null = null;
  unitsToSend: { [unitType: string]: number } = {};

  loading = true;
  message = '';
  error = '';

  constructor(
    private military: MilitaryService,
    private kingdomService: KingdomService,
    private coalitionService: CoalitionService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.kingdomService.getMyKingdom().subscribe(k => this.myKingdom = k);
    this.military.getMyArmy().subscribe(a => this.myUnits = a.filter(u => u.quantity > 0 && !u.unitType.endsWith('_Zlodziej')));
    this.coalitionService.getWars().subscribe({
      next: (wars: War[]) => {
        this.enemyCoalitionIds = new Set(wars.filter(w => w.opponentCoalitionId).map(w => w.opponentCoalitionId));
        this.enemyCoalitionNames = Array.from(new Set(wars.filter(w => w.opponentName).map(w => w.opponentName)));
      },
      error: () => {}
    });
    this.kingdomService.getAllKingdoms().subscribe({
      next: k => { this.allKingdoms = k; this.loading = false; },
      error: () => { this.loading = false; this.error = this.translate.instant('atk.errLoad'); }
    });
  }

  private evaluate(k: KingdomSummary): TargetRow {
    const my = this.myKingdom;
    let attackable = true;
    let reason = 'atk.reason.attackable';
    if (k.isProtected) { attackable = false; reason = 'atk.reason.protected'; }
    else if (k.isFrozen) { attackable = false; reason = 'atk.reason.frozen'; }
    else if (my && k.land > my.land * 4) { attackable = false; reason = 'atk.reason.tooBig'; }
    else if (my && k.land * 4 < my.land) { attackable = false; reason = 'atk.reason.tooSmall'; }
    else if (k.coalitionId && my && k.coalitionId === my.coalitionId) { attackable = false; reason = 'atk.reason.ally'; }
    else if (k.coalitionId && !this.enemyCoalitionIds.has(k.coalitionId)) { attackable = false; reason = 'atk.reason.noWar'; }
    return { k, attackable, reason: this.translate.instant(reason) };
  }

  get coalitionTags(): string[] {
    return Array.from(new Set(this.allKingdoms.map(k => k.coalitionTag).filter((t): t is string => !!t)));
  }

  get targets(): TargetRow[] {
    return this.allKingdoms
      .filter(k => k.id !== this.myKingdom?.id)
      .filter(k => this.coalitionFilter === 'all' || k.coalitionTag === this.coalitionFilter)
      .map(k => this.evaluate(k))
      .filter(r => this.filterMode === 'all' || r.attackable)
      .sort((a, b) => Number(b.attackable) - Number(a.attackable) || b.k.land - a.k.land);
  }

  selectTarget(k: KingdomSummary): void {
    this.selectedTarget = k;
    this.unitsToSend = {};
  }

  get totalSelected(): number {
    return Object.values(this.unitsToSend).reduce((s, n) => s + (Number(n) || 0), 0);
  }

  launchAttack(): void {
    this.message = '';
    this.error = '';
    if (!this.selectedTarget) { this.error = 'Wybierz cel.'; return; }
    const units: { [k: string]: number } = {};
    for (const u of this.myUnits) {
      const q = Number(this.unitsToSend[u.unitType]) || 0;
      if (q > 0) units[u.unitType] = q;
    }
    if (Object.keys(units).length === 0) { this.error = 'Wybierz jednostki do ataku.'; return; }

    this.military.attack(this.selectedTarget.id, units).subscribe({
      next: r => {
        this.message = r.message || 'Atak zakolejkowany.';
        this.selectedTarget = null;
        this.unitsToSend = {};
        this.military.getMyArmy().subscribe(a => this.myUnits = a.filter(u => u.quantity > 0 && !u.unitType.endsWith('_Zlodziej')));
      },
      error: e => { this.error = e.error?.message || e.error || 'Błąd ataku.'; }
    });
  }
}
