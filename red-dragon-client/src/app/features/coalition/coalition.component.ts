import { Component, OnInit } from '@angular/core';
import { CoalitionService } from '../../core/services/coalition.service';
import { KingdomService } from '../../core/services/kingdom.service';
import { Coalition, Kingdom } from '../../core/models/kingdom.model';

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

  constructor(private coalitionService: CoalitionService, private kingdomService: KingdomService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.kingdomService.getMyKingdom().subscribe(k => this.kingdom = k);
    this.coalitionService.getCoalitions().subscribe(c => { this.coalitions = c; this.loading = false; });
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

  private clearMsg(): void { setTimeout(() => this.message = '', 4000); }
}
