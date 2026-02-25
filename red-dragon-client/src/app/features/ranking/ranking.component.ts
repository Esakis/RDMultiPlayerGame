import { Component, OnInit } from '@angular/core';
import { KingdomService } from '../../core/services/kingdom.service';
import { KingdomSummary } from '../../core/models/kingdom.model';

@Component({
  selector: 'app-ranking',
  templateUrl: './ranking.component.html',
  styleUrls: ['./ranking.component.scss']
})
export class RankingComponent implements OnInit {
  kingdoms: KingdomSummary[] = [];
  loading = true;

  constructor(private kingdomService: KingdomService) {}

  ngOnInit(): void {
    this.kingdomService.getAllKingdoms().subscribe({
      next: (k) => {
        this.kingdoms = k.sort((a, b) => b.land - a.land);
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }
}
