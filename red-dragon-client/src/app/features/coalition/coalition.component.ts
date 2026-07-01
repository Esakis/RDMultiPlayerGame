import { Component, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { CoalitionService, PpsStatus, War, Election, Treasury, Announcement } from '../../core/services/coalition.service';
import { KingdomService } from '../../core/services/kingdom.service';
import { Coalition, Kingdom, KingdomSummary } from '../../core/models/kingdom.model';

type PolTab = 'twoja' | 'tablica' | 'wojny' | 'pps' | 'koalicje';

@Component({
  selector: 'app-coalition',
  templateUrl: './coalition.component.html',
  styleUrls: ['./coalition.component.scss']
})
export class CoalitionComponent implements OnInit {
  // Aktywna zakładka sekcji Polityka (układ jak w Budowie).
  activeTab: PolTab = 'twoja';
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
  election: Election | null = null;
  treasury: Treasury | null = null;
  depGold = 0; depBudulec = 0;
  wdrGold = 0; wdrBudulec = 0;
  fundPpsAmount = 0;
  selectedCoalitionId: number | null = null;

  // Tablica ogłoszeń
  announcements: Announcement[] = [];
  annEditId: number | null = null;   // null = nowy wpis
  annTitle = '';
  annContent = '';
  annFormOpen = false;

  // Wspólne hasło koalicji
  sharedPwdStatus: { enabled: boolean; canManage: boolean } | null = null;
  sharedPwd = '';

  constructor(private coalitionService: CoalitionService, private kingdomService: KingdomService,
              private translate: TranslateService) {}

  setTab(tab: PolTab): void { this.activeTab = tab; }

  selectCoalition(id: number): void {
    this.selectedCoalitionId = id;
  }

  /** Powrót z widoku pojedynczej koalicji do listy. */
  clearSelection(): void {
    this.selectedCoalitionId = null;
  }

  /** Aktualnie wybrana (otwarta na pełny widok) koalicja. */
  get selectedCoalition(): Coalition | null {
    return this.coalitions.find(c => c.id === this.selectedCoalitionId) || null;
  }

  /** Siła wojskowa = suma ataku i obrony. */
  memberMilitary(m: KingdomSummary): number {
    return (m.attackPower ?? 0) + (m.defensePower ?? 0);
  }

  /** Łączna siła wojskowa koalicji (Σ atak + obrona członków). */
  coalitionMilitary(c: Coalition): number {
    return c.members.reduce((s, m) => s + this.memberMilitary(m), 0);
  }

  /** Łączna siła magiczna koalicji (Σ many członków). */
  coalitionMagic(c: Coalition): number {
    return c.members.reduce((s, m) => s + (m.magic ?? 0), 0);
  }

  /** Łączna siła złodziejska koalicji (Σ siły złodziejskiej członków). */
  coalitionThief(c: Coalition): number {
    return c.members.reduce((s, m) => s + (m.thiefPower ?? 0), 0);
  }

  /** Etykieta roli członka koalicji (tłumaczona). */
  roleLabel(role: string | null | undefined): string {
    switch (role) {
      case 'Imperator': return this.translate.instant('pol.role.imperator');
      case 'MainCommander': return this.translate.instant('pol.role.commander');
      default: return this.translate.instant('pol.role.member');
    }
  }

  /** Klasa CSS koloru roli. */
  roleClass(role: string | undefined): string {
    switch (role) {
      case 'Imperator': return 'role-imperator';
      case 'MainCommander': return 'role-gd';
      default: return 'role-member';
    }
  }

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
    if (!this.kingdom?.coalitionId) { this.pps = null; this.wars = []; this.election = null; this.announcements = []; return; }
    this.coalitionService.getPps().subscribe({ next: p => this.pps = p, error: () => this.pps = null });
    this.coalitionService.getWars().subscribe({ next: w => this.wars = w, error: () => this.wars = [] });
    this.coalitionService.getElection().subscribe({ next: e => this.election = e, error: () => this.election = null });
    this.coalitionService.getTreasury().subscribe({ next: t => this.treasury = t, error: () => this.treasury = null });
    this.coalitionService.getAnnouncements().subscribe({ next: a => this.announcements = a, error: () => this.announcements = [] });
    this.coalitionService.getSharedPasswordStatus().subscribe({ next: s => this.sharedPwdStatus = s, error: () => this.sharedPwdStatus = null });
  }

  // ===== Wspólne hasło koalicji =====

  setSharedPassword(): void {
    if (!this.sharedPwd) return;
    this.coalitionService.setSharedPassword(this.sharedPwd).subscribe({
      next: res => {
        this.message = res.message || 'OK';
        this.sharedPwd = '';
        this.coalitionService.getSharedPasswordStatus().subscribe(s => this.sharedPwdStatus = s);
        this.clearMsg();
      },
      error: err => { this.message = err.error?.message || err.error || 'Błąd'; this.clearMsg(); }
    });
  }

  clearSharedPassword(): void {
    if (!confirm(this.translate.instant('pol.sharedPwd.confirmClear'))) return;
    this.coalitionService.setSharedPassword(null).subscribe({
      next: res => {
        this.message = res.message || 'OK';
        this.coalitionService.getSharedPasswordStatus().subscribe(s => this.sharedPwdStatus = s);
        this.clearMsg();
      },
      error: err => { this.message = err.error?.message || err.error || 'Błąd'; this.clearMsg(); }
    });
  }

  // ===== Tablica ogłoszeń =====

  /** Czy zalogowany gracz może edytować tablicę (Imperator/Głównodowodzący). */
  get canEditAnnouncements(): boolean {
    return this.kingdom?.coalitionRole === 'Imperator' || this.kingdom?.coalitionRole === 'MainCommander';
  }

  openAnnForm(a?: Announcement): void {
    this.annFormOpen = true;
    this.annEditId = a?.id ?? null;
    this.annTitle = a?.title ?? '';
    this.annContent = a?.contentHtml ?? '';
  }

  closeAnnForm(): void {
    this.annFormOpen = false;
    this.annEditId = null;
    this.annTitle = '';
    this.annContent = '';
  }

  saveAnnouncement(): void {
    if (!this.annContent.trim()) { this.message = this.translate.instant('pol.ann.errEmpty'); this.clearMsg(); return; }
    const req = this.annEditId == null
      ? this.coalitionService.createAnnouncement(this.annTitle || null, this.annContent)
      : this.coalitionService.updateAnnouncement(this.annEditId, this.annTitle || null, this.annContent);
    req.subscribe({
      next: res => {
        this.message = res.message || 'OK';
        this.closeAnnForm();
        this.coalitionService.getAnnouncements().subscribe(a => this.announcements = a);
        this.clearMsg();
      },
      error: err => { this.message = err.error?.message || err.error || 'Błąd'; this.clearMsg(); }
    });
  }

  deleteAnnouncement(a: Announcement): void {
    if (!confirm(this.translate.instant('pol.ann.confirmDelete'))) return;
    this.coalitionService.deleteAnnouncement(a.id).subscribe({
      next: res => {
        this.message = res.message || 'OK';
        this.coalitionService.getAnnouncements().subscribe(x => this.announcements = x);
        this.clearMsg();
      },
      error: err => { this.message = err.error?.message || err.error || 'Błąd'; this.clearMsg(); }
    });
  }

  private treasuryOp(obs: any): void {
    obs.subscribe({
      next: (res: any) => { this.message = res.message || ''; this.load(); this.clearMsg(); },
      error: (err: any) => { this.message = err.error?.message || err.error || 'Błąd'; this.clearMsg(); }
    });
  }

  deposit(): void {
    if (this.depGold <= 0 && this.depBudulec <= 0) { this.message = 'Podaj kwotę.'; this.clearMsg(); return; }
    this.treasuryOp(this.coalitionService.depositTreasury(this.depGold || 0, this.depBudulec || 0));
    this.depGold = 0; this.depBudulec = 0;
  }

  withdraw(): void {
    if (this.wdrGold <= 0 && this.wdrBudulec <= 0) { this.message = 'Podaj kwotę.'; this.clearMsg(); return; }
    this.treasuryOp(this.coalitionService.withdrawTreasury(this.wdrGold || 0, this.wdrBudulec || 0));
    this.wdrGold = 0; this.wdrBudulec = 0;
  }

  fundPps(): void {
    if (this.fundPpsAmount <= 0) { this.message = 'Podaj ilość budulca.'; this.clearMsg(); return; }
    this.treasuryOp(this.coalitionService.fundPpsFromTreasury(this.fundPpsAmount));
    this.fundPpsAmount = 0;
  }

  vote(candidateKingdomId: number): void {
    this.coalitionService.vote(candidateKingdomId).subscribe({
      next: res => { this.message = res.message || 'Głos oddany.'; this.load(); this.clearMsg(); },
      error: err => { this.message = err.error?.message || err.error || 'Błąd'; this.clearMsg(); }
    });
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
      next: (res) => { this.message = res.message || 'Dołączono!'; this.selectedCoalitionId = null; this.load(); this.clearMsg(); },
      error: (err) => { this.message = err.error?.message || err.error || 'Błąd'; this.clearMsg(); }
    });
  }

  /** Koalicje, do których można dołączyć (inne niż moja, z wolnym miejscem). */
  get joinableCoalitions(): Coalition[] {
    return this.coalitions.filter(c => c.id !== this.kingdom?.coalitionId && c.memberCount < c.maxMembers);
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

  private clearMsg(): void { setTimeout(() => this.message = '', 4000); }
}
