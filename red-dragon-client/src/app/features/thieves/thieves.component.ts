import { Component, OnInit } from '@angular/core';
import { ThiefService, ThiefActionItem } from '../../core/services/thief.service';
import { KingdomService } from '../../core/services/kingdom.service';
import { KingdomSummary } from '../../core/models/kingdom.model';

@Component({
  selector: 'app-thieves',
  templateUrl: './thieves.component.html',
  styleUrls: ['./thieves.component.scss']
})
export class ThievesComponent implements OnInit {
  actions: ThiefActionItem[] = [];
  kingdoms: KingdomSummary[] = [];
  selectedAction?: ThiefActionItem;
  targetId?: number;
  thieves = 0;
  message = '';
  error = '';
  loading = true;

  constructor(private thief: ThiefService, private kingdomService: KingdomService) {}

  ngOnInit(): void {
    this.thief.getActions().subscribe({
      next: a => { this.actions = a; this.loading = false; },
      error: () => { this.loading = false; }
    });
    this.kingdomService.getAllKingdoms().subscribe(k => this.kingdoms = k);
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
