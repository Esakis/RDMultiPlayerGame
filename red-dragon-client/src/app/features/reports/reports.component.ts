import { Component, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { MilitaryService } from '../../core/services/military.service';
import { KingdomService } from '../../core/services/kingdom.service';
import { Kingdom, BattleReport } from '../../core/models/kingdom.model';

@Component({
  selector: 'app-reports',
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.scss']
})
export class ReportsComponent implements OnInit {
  tab: 'mine' | 'coalition' = 'mine';
  myKingdom: Kingdom | null = null;
  myReports: BattleReport[] = [];
  coalitionReports: BattleReport[] = [];
  coalitionLoaded = false;

  loading = true;
  error = '';

  constructor(
    private military: MilitaryService,
    private kingdomService: KingdomService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.kingdomService.getMyKingdom().subscribe(k => this.myKingdom = k);
    this.military.getBattleReports().subscribe({
      next: r => { this.myReports = r; this.loading = false; },
      error: () => { this.loading = false; this.error = this.translate.instant('rpt.errLoad'); }
    });
  }

  setTab(tab: 'mine' | 'coalition'): void {
    this.tab = tab;
    if (tab === 'coalition' && !this.coalitionLoaded) {
      this.loading = true;
      this.military.getCoalitionBattleReports().subscribe({
        next: r => { this.coalitionReports = r; this.coalitionLoaded = true; this.loading = false; },
        error: () => { this.loading = false; this.error = this.translate.instant('rpt.errLoad'); }
      });
    }
  }

  get reports(): BattleReport[] {
    return this.tab === 'mine' ? this.myReports : this.coalitionReports;
  }

  /** Czy raport dotyczy obrony mojego księstwa (ja jestem celem). */
  isDefense(r: BattleReport): boolean {
    return !!this.myKingdom && r.defenderKingdomId === this.myKingdom.id;
  }

  resultKey(r: BattleReport): string {
    return 'rpt.result.' + r.result;
  }

  resultGood(r: BattleReport): boolean {
    const attackerSucceeded = r.result === 'Victory' || r.result === 'Success';
    return this.isDefense(r) ? !attackerSucceeded : attackerSucceeded;
  }

  /** Buduje czytelne linie szczegółów z JSON-owych pól raportu. */
  details(r: BattleReport): string[] {
    const lines: string[] = [];
    const t = (key: string, params?: object) => this.translate.instant(key, params);

    if (r.landCaptured > 0) lines.push(t('rpt.landCaptured', { n: r.landCaptured }));

    const stolen = this.parse(r.resourcesStolen);
    if (stolen) {
      const parts = Object.entries(stolen)
        .filter(([, v]) => Number(v) > 0)
        .map(([k, v]) => `${t('rpt.res.' + k.toLowerCase())}: ${Number(v).toLocaleString()}`);
      if (parts.length) lines.push(t('rpt.stolen') + ' ' + parts.join(', '));
    }

    const att = this.parse(r.attackerLosses);
    if (att) {
      if ('spell' in att) lines.push(t('rpt.spell') + ': ' + att['spell']);
      else if ('action' in att) {
        lines.push(t('rpt.action') + ': ' + att['action']
          + (att['thievesLost'] != null ? ` (${t('rpt.thievesLost', { n: att['thievesLost'] })})` : ''));
      } else {
        const parts = this.unitLines(att);
        if (parts.length) lines.push(t('rpt.attLosses') + ' ' + parts.join(', '));
      }
    }

    const def = this.parse(r.defenderLosses);
    if (def) {
      if ('info' in def) lines.push(String(def['info']));
      else {
        const parts = this.unitLines(def);
        if (parts.length) lines.push(t('rpt.defLosses') + ' ' + parts.join(', '));
      }
    }

    return lines;
  }

  private unitLines(losses: { [k: string]: any }): string[] {
    return Object.entries(losses)
      .filter(([, v]) => Number(v) > 0)
      .map(([unitType, n]) => `${unitType.split('_').pop()} ×${Number(n).toLocaleString()}`);
  }

  private parse(json: string | null): { [k: string]: any } | null {
    if (!json) return null;
    try {
      const value = JSON.parse(json);
      return value && typeof value === 'object' ? value : null;
    } catch {
      return null;
    }
  }
}
