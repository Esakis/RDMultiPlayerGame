import { Component, OnInit } from '@angular/core';
import { KingdomService } from '../../core/services/kingdom.service';
import { Kingdom } from '../../core/models/kingdom.model';

@Component({
  selector: 'app-economy',
  templateUrl: './economy.component.html',
  styleUrls: ['./economy.component.scss']
})
export class EconomyComponent implements OnInit {
  kingdom: Kingdom | null = null;
  loading = true;
  message = '';
  assignAmounts: { [key: string]: number } = {};
  landAmount = 1;

  constructor(private kingdomService: KingdomService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.kingdomService.getMyKingdom().subscribe({
      next: (k) => { this.kingdom = k; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  assign(profType: string, amount: number): void {
    this.kingdomService.assignWorkers({ professionType: profType, workerCount: amount }).subscribe({
      next: (res) => { this.message = res.message || 'OK'; this.load(); this.clearMsg(); },
      error: (err) => { this.message = err.error?.message || err.error || 'Błąd'; this.clearMsg(); }
    });
  }

  unassign(profType: string, amount: number): void {
    this.assign(profType, -amount);
  }

  buyLand(): void {
    if (this.landAmount <= 0) return;
    this.kingdomService.buyLand(this.landAmount).subscribe({
      next: (res) => { this.message = res.message || 'OK'; this.load(); this.clearMsg(); },
      error: (err) => { this.message = err.error?.message || err.error || 'Błąd'; this.clearMsg(); }
    });
  }

  getProductionInfo(profType: string): string {
    const map: {[k:string]:string} = {
      'Unemployed': '-',
      'Stonemason': '+100 🪨/turę (z kamieniołomem)',
      'Builder': 'Przyśpiesza budowę',
      'Merchant': '+10 💰/turę',
      'Alchemist': '+20 💰/turę',
      'Druid': '+5 🔮/turę',
      'Mage': 'Czary bojowe',
      'Scientist': 'Przyśpiesza badania',
      'Soldier': 'Obrona'
    };
    return map[profType] || '-';
  }

  private clearMsg(): void { setTimeout(() => this.message = '', 4000); }
}
