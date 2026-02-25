import { Component, OnInit } from '@angular/core';
import { MilitaryService } from '../../core/services/military.service';
import { KingdomService } from '../../core/services/kingdom.service';
import { UnitDefinition, MilitaryUnit, KingdomSummary } from '../../core/models/kingdom.model';

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

  constructor(private militaryService: MilitaryService, private kingdomService: KingdomService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.militaryService.getAvailableUnits().subscribe(u => {
      this.unitDefs = u;
      u.forEach(d => { if (!this.recruitQty[d.unitType]) this.recruitQty[d.unitType] = 1; });
    });
    this.militaryService.getMyArmy().subscribe(a => { this.myArmy = a; this.loading = false; });
    this.kingdomService.getAllKingdoms().subscribe(k => this.kingdoms = k);
  }

  recruit(unitType: string): void {
    const qty = this.recruitQty[unitType] || 1;
    this.militaryService.recruit({ unitType, quantity: qty }).subscribe({
      next: (res) => { this.message = res.message || 'OK'; this.load(); this.clearMsg(); },
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

  private clearMsg(): void { setTimeout(() => this.message = '', 4000); }
}
