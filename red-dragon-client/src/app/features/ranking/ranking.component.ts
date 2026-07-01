import { Component, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { KingdomService } from '../../core/services/kingdom.service';
import { CoalitionService, PantheonEntry } from '../../core/services/coalition.service';
import { Kingdom, KingdomSummary } from '../../core/models/kingdom.model';

@Component({
  selector: 'app-ranking',
  templateUrl: './ranking.component.html',
  styleUrls: ['./ranking.component.scss']
})
export class RankingComponent implements OnInit {
  kingdoms: KingdomSummary[] = [];
  loading = true;

  // Aktywna zakładka: statystyki własnej koalicji / ranking wszystkich księstw / Panteon.
  activeTab: 'koalicja' | 'ranking' | 'panteon' = 'ranking';

  // Statystyki własnej koalicji
  kingdom: Kingdom | null = null;
  coalitionName: string | null = null;
  coalitionMembers: KingdomSummary[] = [];

  // Panteon — sala chwały zakończonych er
  pantheon: PantheonEntry[] = [];
  pantheonLoaded = false;

  constructor(
    private kingdomService: KingdomService,
    private coalitionService: CoalitionService,
    private translate: TranslateService
  ) {}

  setTab(tab: 'koalicja' | 'ranking' | 'panteon'): void {
    this.activeTab = tab;
    if (tab === 'panteon' && !this.pantheonLoaded) {
      this.coalitionService.getPantheon().subscribe({
        next: p => { this.pantheon = p; this.pantheonLoaded = true; },
        error: () => {}
      });
    }
  }

  ngOnInit(): void {
    this.kingdomService.getAllKingdoms().subscribe({
      next: (k) => {
        this.kingdoms = k.sort((a, b) => b.land - a.land);
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });

    this.loadCoalitionStats();
  }

  private loadCoalitionStats(): void {
    this.kingdomService.getMyKingdom().subscribe(k => {
      this.kingdom = k;
      if (!k?.coalitionId) return;
      this.coalitionName = k.coalitionName ?? null;
      this.coalitionService.getCoalitions().subscribe(coalitions => {
        const mine = coalitions.find(c => c.id === k.coalitionId);
        this.coalitionMembers = mine ? mine.members : [];
      });
    });
  }

  getMemberRoleClass(role?: string): string {
    switch (role) {
      case 'Imperator': return 'imperator';
      case 'MainCommander': return 'main-commander';
      default: return '';
    }
  }

  getRoleDisplay(role?: string): string {
    switch (role) {
      case 'Imperator': return `[${this.translate.instant('pol.role.imperator')}]`;
      case 'MainCommander': return `[${this.translate.instant('pol.role.commander')}]`;
      default: return '';
    }
  }

  private sum(pick: (m: KingdomSummary) => number | undefined): number {
    return this.coalitionMembers.reduce((s, m) => s + (pick(m) || 0), 0);
  }

  get totalLand(): number { return this.sum(m => m.land); }
  get totalPopulation(): number { return this.sum(m => m.population); }
  get totalGold(): number { return this.sum(m => m.gold); }
  get totalMilitary(): number { return this.sum(m => m.military); }
  get totalAttack(): number { return this.sum(m => m.attackPower); }
  get totalDefense(): number { return this.sum(m => m.defensePower); }
  get totalMagic(): number { return this.sum(m => m.magic); }
  get totalThief(): number { return this.sum(m => m.thiefPower); }
  get totalBuildings(): number { return this.sum(m => m.buildingCount); }
  get totalUsedLand(): number { return this.sum(m => m.usedLand); }
  get totalFreeLand(): number { return this.sum(m => m.freeLand); }
  get totalBuiltPercent(): number {
    return this.totalLand > 0 ? Math.round(this.totalUsedLand / this.totalLand * 1000) / 10 : 0;
  }
}
