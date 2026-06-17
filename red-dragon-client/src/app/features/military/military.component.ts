import { Component, OnInit } from '@angular/core';
import { MilitaryService } from '../../core/services/military.service';
import { KingdomService } from '../../core/services/kingdom.service';
import { UnitDefinition, MilitaryUnit, KingdomSummary, TrainingInfo } from '../../core/models/kingdom.model';

@Component({
  selector: 'app-military',
  templateUrl: './military.component.html',
  styleUrls: ['./military.component.scss']
})
export class MilitaryComponent implements OnInit {
  unitDefs: UnitDefinition[] = [];
  myArmy: MilitaryUnit[] = [];
  kingdoms: KingdomSummary[] = [];
  loading = true;
  message = '';
  recruitQty: { [key: string]: number } = {};
  attackTarget = 0;
  attackUnits: { [key: string]: number } = {};
  training: TrainingInfo = {
    trainSoldiers: false, trainElite: false,
    soldierPromotePct: 0, elitePromotePct: 0,
    canTrainSoldiers: false, canTrainElite: false
  };

  // Kolejność slotów jak na oryginalnej zakładce wojska (docs/zrodla/grafiki/wojsko.png):
  // Żołnierz, Elita 1, Elita 2 / Złodziej, Machina, Smok
  private readonly slotOrder = ['hoplita', 'elita1', 'elita2', 'zlodziej', 'machina', 'smok'];

  constructor(private militaryService: MilitaryService, private kingdomService: KingdomService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.militaryService.getAvailableUnits().subscribe(u => {
      this.unitDefs = [...u].sort((a, b) => this.slotRank(a) - this.slotRank(b));
    });
    this.militaryService.getMyArmy().subscribe(a => { this.myArmy = a; this.loading = false; });
    this.kingdomService.getAllKingdoms().subscribe(k => this.kingdoms = k);
    this.militaryService.getTraining().subscribe(t => this.training = t);
  }

  // Pojedyncza rekrutacja zastąpiona zbiorczym przyciskiem „Rekrutuj" (recruitAll).

  /** Zbiera wpisane w pola ilości (>0). */
  private collectQty(onlyRecruitable: boolean): { [key: string]: number } {
    const units: { [key: string]: number } = {};
    for (const def of this.unitDefs) {
      const qty = this.recruitQty[def.unitType];
      if (!qty || qty <= 0) continue;
      if (onlyRecruitable && !def.canRecruit) continue;
      units[def.unitType] = qty;
    }
    return units;
  }

  /** Jeden przycisk „Rekrutuj" — rekrutuje wszystkie wpisane ilości naraz. */
  recruitAll(): void {
    const units = this.collectQty(true);
    if (Object.keys(units).length === 0) { this.message = 'Wpisz ilości do rekrutacji.'; this.clearMsg(); return; }
    this.militaryService.recruitBatch(units).subscribe({
      next: (res) => { this.message = res.message || 'OK'; this.recruitQty = {}; this.load(); this.clearMsg(); },
      error: (err) => { this.message = err.error?.message || err.error || 'Błąd'; this.clearMsg(); }
    });
  }

  /** Jeden przycisk „Zwolnij" — rozwiązuje wszystkie wpisane ilości naraz. */
  disbandAll(): void {
    const units = this.collectQty(false);
    if (Object.keys(units).length === 0) { this.message = 'Wpisz ilości do zwolnienia.'; this.clearMsg(); return; }
    this.militaryService.disband(units).subscribe({
      next: (res) => { this.message = res.message || 'OK'; this.recruitQty = {}; this.load(); this.clearMsg(); },
      error: (err) => { this.message = err.error?.message || err.error || 'Błąd'; this.clearMsg(); }
    });
  }

  /** Przełącza szkolenie żołnierzy/elity i zapisuje na serwerze. */
  toggleTraining(): void {
    this.militaryService.setTraining({
      trainSoldiers: this.training.trainSoldiers,
      trainElite: this.training.trainElite
    }).subscribe({
      next: () => this.militaryService.getTraining().subscribe(t => this.training = t),
      error: (err) => { this.message = err.error?.message || err.error || 'Błąd'; this.clearMsg(); }
    });
  }

  sendAttack(): void {
    if (!this.attackTarget) { this.message = 'Wybierz cel ataku.'; this.clearMsg(); return; }
    const units: { [key: string]: number } = {};
    for (const key of Object.keys(this.attackUnits)) {
      if (this.attackUnits[key] > 0) units[key] = this.attackUnits[key];
    }
    if (Object.keys(units).length === 0) { this.message = 'Wybierz jednostki do ataku.'; this.clearMsg(); return; }
    this.militaryService.attack(this.attackTarget, units).subscribe({
      next: (res) => { this.message = res.message || 'Atak zakolejkowany!'; this.load(); this.clearMsg(); },
      error: (err) => { this.message = err.error?.message || err.error || 'Błąd'; this.clearMsg(); }
    });
  }

  getOwned(unitType: string): MilitaryUnit | undefined {
    return this.myArmy.find(u => u.unitType === unitType);
  }

  /** Slot jednostki niezależny od rasy (Hoplita/Elita1/Elita2/Złodziej/Machina/Smok). */
  slotKey(def: UnitDefinition): string {
    const t = def.unitType;
    // Ożywieńcy (Nekromant) nie mają machin — w tym slocie stoi Drakolicz.
    if (t === 'Nekromant_Machina') return 'drakolicz';
    if (t.endsWith('_Hoplita')) return 'hoplita';
    if (t.endsWith('_Machina')) return 'machina';
    if (t.endsWith('_Zlodziej')) return 'zlodziej';
    if (t.endsWith('_Smok')) return 'smok';
    if (def.description?.includes('Elita 2')) return 'elita2';
    if (def.description?.includes('Elita 1')) return 'elita1';
    return 'hoplita';
  }

  /** Domyślna grafika jednostki dla danego slotu. */
  unitImage(def: UnitDefinition): string {
    return `assets/img/wojsko/${this.slotKey(def)}.png`;
  }

  /** Nazwa wyświetlana — Drakolicz zamiast machiny u Ożywieńców. */
  unitName(def: UnitDefinition): string {
    return def.unitType === 'Nekromant_Machina' ? 'Drakolicz' : def.displayName;
  }

  private slotRank(def: UnitDefinition): number {
    const key = this.slotKey(def) === 'drakolicz' ? 'machina' : this.slotKey(def);
    const i = this.slotOrder.indexOf(key);
    return i < 0 ? 99 : i;
  }

  /** Udział jednostki w łącznej liczebności armii (procent jak w oryginale). */
  sharePct(unitType: string): number {
    const total = this.myArmy.reduce((s, u) => s + u.quantity, 0);
    if (!total) return 0;
    const owned = this.getOwned(unitType)?.quantity || 0;
    return Math.round((owned / total) * 100);
  }

  private clearMsg(): void { setTimeout(() => this.message = '', 4000); }
}
