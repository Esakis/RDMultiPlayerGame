import { Component, OnInit } from '@angular/core';
import { ThiefService, ThiefActionItem } from '../../core/services/thief.service';
import { KingdomService } from '../../core/services/kingdom.service';
import { CoalitionService, War } from '../../core/services/coalition.service';
import { KingdomSummary, Kingdom } from '../../core/models/kingdom.model';

@Component({
  selector: 'app-thieves',
  templateUrl: './thieves.component.html',
  styleUrls: ['./thieves.component.scss']
})
export class ThievesComponent implements OnInit {
  actions: ThiefActionItem[] = [];
  kingdoms: KingdomSummary[] = [];
  myKingdom: Kingdom | null = null;
  enemyCoalitionIds = new Set<number>();
  selectedAction?: ThiefActionItem;
  targetId?: number;
  thieves = 0;
  message = '';
  error = '';
  loading = true;

  constructor(
    private thief: ThiefService,
    private kingdomService: KingdomService,
    private coalitionService: CoalitionService
  ) {}

  ngOnInit(): void {
    this.thief.getActions().subscribe({
      next: a => { this.actions = a; this.loading = false; },
      error: () => { this.loading = false; }
    });
    this.kingdomService.getAllKingdoms().subscribe(k => this.kingdoms = k);
    this.kingdomService.getMyKingdom().subscribe(k => this.myKingdom = k);
    this.coalitionService.getWars().subscribe({
      next: (wars: War[]) => this.enemyCoalitionIds =
        new Set(wars.filter(w => w.opponentCoalitionId).map(w => w.opponentCoalitionId)),
      error: () => {}
    });
  }

  /** Złodziei można wysłać tylko na księstwa koalicji będących z nami w stanie wojny/zasadzki. */
  get targets(): KingdomSummary[] {
    return this.kingdoms.filter(k =>
      k.id !== this.myKingdom?.id
      && !k.isProtected && !k.isFrozen
      && !!k.coalitionId && this.enemyCoalitionIds.has(k.coalitionId));
  }

  select(action: ThiefActionItem): void {
    this.selectedAction = action;
    if (this.thieves < action.thievesRequired) {
      this.thieves = action.thievesRequired;
    }
  }

  send(): void {
    this.message = '';
    this.error = '';
    if (!this.selectedAction) { this.error = 'Wybierz akcję.'; return; }
    if (!this.targetId) { this.error = 'Wybierz cel.'; return; }
    this.thief.send(this.selectedAction.actionType, this.targetId, this.thieves).subscribe({
      next: r => { this.message = r.message ?? ''; },
      error: e => { this.error = e.error || 'Błąd wysyłania złodziei.'; }
    });
  }
}
