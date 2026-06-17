import { Component, OnInit } from '@angular/core';
import { CoalitionService } from '../../core/services/coalition.service';
import { KingdomService } from '../../core/services/kingdom.service';
import { Kingdom } from '../../core/models/kingdom.model';

@Component({
  selector: 'app-options',
  templateUrl: './options.component.html',
  styleUrls: ['./options.component.scss']
})
export class OptionsComponent implements OnInit {
  kingdom: Kingdom | null = null;
  loading = true;
  message = '';

  constructor(private coalitionService: CoalitionService, private kingdomService: KingdomService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.kingdomService.getMyKingdom().subscribe(k => { this.kingdom = k; this.loading = false; });
  }

  leaveCoalition(): void {
    this.coalitionService.leave().subscribe({
      next: (res) => { this.message = res.message || 'Opuszczono koalicję.'; this.load(); this.clearMsg(); },
      error: (err) => { this.message = err.error?.message || err.error || 'Błąd'; this.clearMsg(); }
    });
  }

  toggleFreeze(): void {
    if (!this.kingdom) return;
    const op = this.kingdom.isFrozen
      ? this.kingdomService.unfreeze()
      : this.kingdomService.freeze();
    op.subscribe({
      next: (res) => { this.message = res.message || ''; this.load(); this.clearMsg(); },
      error: (err) => { this.message = err.error?.message || err.error || 'Błąd'; this.clearMsg(); }
    });
  }

  dropProtection(): void {
    if (!this.kingdom) return;
    this.kingdomService.dropProtection().subscribe({
      next: (res) => { this.message = res.message || ''; this.load(); this.clearMsg(); },
      error: (err) => { this.message = err.error?.message || err.error || 'Błąd'; this.clearMsg(); }
    });
  }

  private clearMsg(): void { setTimeout(() => this.message = '', 4000); }
}
