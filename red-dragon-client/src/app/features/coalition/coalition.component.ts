import { Component, OnInit } from '@angular/core';
import { CoalitionService, PpsStatus, War } from '../../core/services/coalition.service';
import { KingdomService } from '../../core/services/kingdom.service';
import { Coalition, Kingdom, KingdomSummary } from '../../core/models/kingdom.model';

@Component({
  selector: 'app-coalition',
  templateUrl: './coalition.component.html',
  styleUrls: ['./coalition.component.scss']
})
export class CoalitionComponent implements OnInit {
  coalitions: Coalition[] = [];
  kingdom: Kingdom | null = null;
  loading = true;
  message = '';
  newName = '';
  newTag = '';
  selectedCommanderId: number | null = null;
  coalitionMembers: KingdomSummary[] = [];
  currentMainCommander: KingdomSummary | null = null;
  pps: PpsStatus | null = null;
  contributeAmount = 0;
  wars: War[] = [];
  declareTargetId: number | null = null;

  constructor(private coalitionService: CoalitionService, private kingdomService: KingdomService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.kingdomService.getMyKingdom().subscribe(k => {
      this.kingdom = k;
      this.loadCoalitionMembers();
      this.loadPps();
    });
    this.coalitionService.getCoalitions().subscribe(c => {
      this.coalitions = c;
      this.loading = false;
      this.loadCoalitionMembers();
    });
  }

  loadPps(): void {
    if (!this.kingdom?.coalitionId) { this.pps = null; this.wars = []; return; }
    this.coalitionService.getPps().subscribe({ next: p => this.pps = p, error: () => this.pps = null });
    this.coalitionService.getWars().subscribe({ next: w => this.wars = w, error: () => this.wars = [] });
  }

  get otherCoalitions(): Coalition[] {
    return this.coalitions.filter(c => c.id !== this.kingdom?.coalitionId);
  }

  declareWar(): void {
    if (!this.declareTargetId) { this.message = 'Wybierz koalicję.'; this.clearMsg(); return; }
    this.coalitionService.declareWar(this.declareTargetId).subscribe({
      next: res => { this.message = res.message || 'Wojna wypowiedziana.'; this.declareTargetId = null; this.load(); this.clearMsg(); },
      error: err => { this.message = err.error?.message || err.error || 'Błąd'; this.clearMsg(); }
    });
  }

  endWar(war: War): void {
    this.coalitionService.endWar(war.id).subscribe({
      next: res => { this.message = res.message || 'Pokój zawarty.'; this.load(); this.clearMsg(); },
      error: err => { this.message = err.error?.message || err.error || 'Błąd'; this.clearMsg(); }
    });
  }

  startPps(): void {
    this.coalitionService.startPps().subscribe({
      next: res => { this.message = res.message || 'Rozpoczęto budowę PPS.'; this.load(); this.clearMsg(); },
      error: err => { this.message = err.error?.message || err.error || 'Błąd'; this.clearMsg(); }
    });
  }

  contributePps(): void {
    if (this.contributeAmount <= 0) { this.message = 'Podaj ilość budulca.'; this.clearMsg(); return; }
    this.coalitionService.contributePps(this.contributeAmount).subscribe({
      next: res => { this.message = res.message || 'Wpłacono budulec.'; this.contributeAmount = 0; this.load(); this.clearMsg(); },
      error: err => { this.message = err.error?.message || err.error || 'Błąd'; this.clearMsg(); }
    });
  }

  loadCoalitionMembers(): void {
    if (!this.kingdom?.coalitionId) return;

    const myCoalition = this.coalitions.find(c => c.id === this.kingdom?.coalitionId);
    if (myCoalition) {
      this.coalitionMembers = myCoalition.members;
      this.currentMainCommander = this.coalitionMembers.find(m => m.coalitionRole === 'MainCommander') || null;
    }
  }

  createCoalition(): void {
    if (!this.newName) { this.message = 'Podaj nazwę koalicji.'; return; }
    this.coalitionService.create(this.newName, this.newTag).subscribe({
      next: (res) => { this.message = res.message || 'Koalicja utworzona!'; this.load(); this.clearMsg(); },
      error: (err) => { this.message = err.error?.message || err.error || 'Błąd'; this.clearMsg(); }
    });
  }

  joinCoalition(id: number): void {
    this.coalitionService.join(id).subscribe({
      next: (res) => { this.message = res.message || 'Dołączono!'; this.load(); this.clearMsg(); },
      error: (err) => { this.message = err.error?.message || err.error || 'Błąd'; this.clearMsg(); }
    });
  }

  leaveCoalition(): void {
    this.coalitionService.leave().subscribe({
      next: (res) => { this.message = res.message || 'Opuszczono.'; this.load(); this.clearMsg(); },
      error: (err) => { this.message = err.error?.message || err.error || 'Błąd'; this.clearMsg(); }
    });
  }

  appointMainCommander(): void {
    if (!this.selectedCommanderId) {
      this.message = 'Wybierz księstwo do mianowania.';
      return;
    }
    this.coalitionService.appointMainCommander(this.selectedCommanderId).subscribe({
      next: (res) => {
        this.message = res.message || 'Głównodowodzący mianowany!';
        this.selectedCommanderId = null;
        this.load();
        this.clearMsg();
      },
      error: (err) => { this.message = err.error?.message || err.error || 'Błąd'; this.clearMsg(); }
    });
  }

  removeMainCommander(): void {
    this.coalitionService.removeMainCommander().subscribe({
      next: (res) => {
        this.message = res.message || 'Głównodowodzący usunięty!';
        this.load();
        this.clearMsg();
      },
      error: (err) => { this.message = err.error?.message || err.error || 'Błąd'; this.clearMsg(); }
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
      case 'Imperator': return '[Imperator]';
      case 'MainCommander': return '[Głównodowodzący]';
      default: return '';
    }
  }

  private clearMsg(): void { setTimeout(() => this.message = '', 4000); }
}
