import { Component, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { MilitaryService, PlannedAttack, AttackOptions, AttackUnit } from '../../core/services/military.service';
import { KingdomService } from '../../core/services/kingdom.service';
import { CoalitionService, War } from '../../core/services/coalition.service';
import { PactService, PactMember } from '../../core/services/pact.service';
import { KingdomSummary, Kingdom } from '../../core/models/kingdom.model';

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
  allKingdoms: KingdomSummary[] = [];
  enemyCoalitionIds = new Set<number>();
  enemyCoalitionNames: string[] = [];

  filterMode: 'all' | 'attackable' = 'attackable';
  coalitionFilter = 'all';

  selectedTarget: KingdomSummary | null = null;
  unitsToSend: { [unitType: string]: number } = {};
  selectedGeneralId: number | null = null;

  // Księstwo, z którego wyrusza atak (Imperator/GD może wybrać dowolne z koalicji)
  attackFromId: number | null = null;
  attackOptions: AttackOptions | null = null;

  plannedAttacks: PlannedAttack[] = [];
  coalitionPlannedAttacks: PlannedAttack[] = [];

  loading = true;
  message = '';
  error = '';

  // Komando łuczników Elfa (blog 31. wieku)
  pactMembers: PactMember[] = [];
  commandoTargetId: number | null = null;

  constructor(
    private military: MilitaryService,
    private kingdomService: KingdomService,
    private coalitionService: CoalitionService,
    private pactService: PactService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.kingdomService.getMyKingdom().subscribe(k => {
      this.myKingdom = k;
      this.attackFromId = k.id;
      this.loadAttackOptions(k.id);
      this.loadPlannedAttacks();
      if (k.race === 'Elf') {
        this.commandoTargetId = k.archerCommandoTargetId;
        this.pactService.getStatus().subscribe({
          next: s => this.pactMembers = s.members,
          error: () => {}
        });
      }
    });
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

  /** Czy zalogowany gracz dowodzi koalicją (Imperator lub Głównodowodzący). */
  get isCommander(): boolean {
    return this.myKingdom?.coalitionRole === 'Imperator' || this.myKingdom?.coalitionRole === 'MainCommander';
  }

  /** Księstwa koalicji, z których dowódca może zaplanować atak. */
  get coalitionMembers(): KingdomSummary[] {
    const myCoalition = this.myKingdom?.coalitionId;
    if (!myCoalition) return [];
    return this.allKingdoms.filter(k => k.coalitionId === myCoalition);
  }

  get availableGenerals(): AttackOptions['generals'] {
    return this.attackOptions?.generals ?? [];
  }

  get attackUnits(): AttackUnit[] {
    return this.attackOptions?.units ?? [];
  }

  onAttackFromChange(): void {
    if (this.attackFromId) {
      this.loadAttackOptions(this.attackFromId);
      this.unitsToSend = {};
      this.selectedGeneralId = null;
    }
  }

  loadAttackOptions(kingdomId: number): void {
    this.military.getAttackOptions(kingdomId).subscribe({
      next: o => {
        this.attackOptions = o;
        this.selectedGeneralId = o.generals.length === 1 ? o.generals[0].id : null;
      },
      error: e => { this.attackOptions = null; this.error = e.error?.message || e.error || ''; }
    });
  }

  loadPlannedAttacks(): void {
    this.military.getPlannedAttacks().subscribe({
      next: p => this.plannedAttacks = p,
      error: () => {}
    });
    if (this.isCommander) {
      this.military.getCoalitionPlannedAttacks().subscribe({
        next: p => this.coalitionPlannedAttacks = p.filter(a => a.attackerKingdomId !== this.myKingdom?.id),
        error: () => {}
      });
    }
  }

  private evaluate(k: KingdomSummary): TargetRow {
    const from = this.allKingdoms.find(x => x.id === this.attackFromId) ?? this.myKingdom;
    let attackable = true;
    let reason = 'atk.reason.attackable';
    if (k.isProtected) { attackable = false; reason = 'atk.reason.protected'; }
    else if (k.isFrozen) { attackable = false; reason = 'atk.reason.frozen'; }
    else if (from && k.land > from.land * 4) { attackable = false; reason = 'atk.reason.tooBig'; }
    else if (from && k.land * 4 < from.land) { attackable = false; reason = 'atk.reason.tooSmall'; }
    else if (k.coalitionId && this.myKingdom && k.coalitionId === this.myKingdom.coalitionId) { attackable = false; reason = 'atk.reason.ally'; }
    else if (k.coalitionId && !this.enemyCoalitionIds.has(k.coalitionId)) { attackable = false; reason = 'atk.reason.noWar'; }
    return { k, attackable, reason: this.translate.instant(reason) };
  }

  get coalitionTags(): string[] {
    return Array.from(new Set(this.allKingdoms.map(k => k.coalitionTag).filter((t): t is string => !!t)));
  }

  get targets(): TargetRow[] {
    return this.allKingdoms
      .filter(k => k.id !== this.attackFromId)
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

  unitsSummary(units: { [unitType: string]: number }): string {
    return Object.entries(units)
      .map(([type, n]) => `${type.split('_').pop()} ×${n}`)
      .join(', ');
  }

  cancelPlanned(id: number): void {
    this.message = '';
    this.error = '';
    this.military.cancelPlannedAttack(id).subscribe({
      next: r => {
        this.message = r.message || this.translate.instant('atk.planned.cancelled');
        this.loadPlannedAttacks();
        if (this.attackFromId) this.loadAttackOptions(this.attackFromId);
      },
      error: e => { this.error = e.error?.message || e.error || this.translate.instant('atk.errAttack'); }
    });
  }

  // ── Komando łuczników Elfa: wsparcie sojusznika z paktem wojskowym ──

  /** Sojusznicy z aktywnym paktem wojskowym, którym można wysłać komando (nie-Elfy). */
  get commandoTargets(): PactMember[] {
    return this.pactMembers.filter(m => m.activePacts.includes('Wojskowy') && m.race !== 'Elf');
  }

  get commandoTargetName(): string {
    const t = this.pactMembers.find(m => m.kingdomId === this.commandoTargetId);
    return t?.name || `#${this.commandoTargetId}`;
  }

  sendCommando(targetKingdomId: number): void {
    this.kingdomService.setArcherCommando(targetKingdomId).subscribe({
      next: r => { this.message = r.message || 'OK'; this.commandoTargetId = targetKingdomId; },
      error: e => { this.error = e.error?.message || e.error || this.translate.instant('atk.errAttack'); }
    });
  }

  cancelCommando(): void {
    this.kingdomService.setArcherCommando(null).subscribe({
      next: r => { this.message = r.message || 'OK'; this.commandoTargetId = null; },
      error: e => { this.error = e.error?.message || e.error || this.translate.instant('atk.errAttack'); }
    });
  }

  launchAttack(): void {
    this.message = '';
    this.error = '';
    if (!this.selectedTarget) { this.error = this.translate.instant('atk.errNoTarget'); return; }
    if (!this.selectedGeneralId) { this.error = this.translate.instant('atk.errNoGeneral'); return; }
    const units: { [k: string]: number } = {};
    for (const u of this.attackUnits) {
      const q = Number(this.unitsToSend[u.unitType]) || 0;
      if (q > 0) units[u.unitType] = q;
    }
    if (Object.keys(units).length === 0) { this.error = this.translate.instant('atk.errNoUnits'); return; }

    const attackerKingdomId = this.attackFromId && this.attackFromId !== this.myKingdom?.id
      ? this.attackFromId
      : undefined;

    this.military.attack(this.selectedTarget.id, this.selectedGeneralId, units, attackerKingdomId).subscribe({
      next: r => {
        this.message = r.message || this.translate.instant('atk.queued');
        this.selectedTarget = null;
        this.unitsToSend = {};
        this.selectedGeneralId = null;
        if (this.attackFromId) this.loadAttackOptions(this.attackFromId);
        this.loadPlannedAttacks();
      },
      error: e => { this.error = e.error?.message || e.error || this.translate.instant('atk.errAttack'); }
    });
  }
}
