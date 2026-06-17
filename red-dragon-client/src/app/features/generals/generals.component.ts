import { Component, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { GeneralService, General } from '../../core/services/general.service';

@Component({
  selector: 'app-generals',
  templateUrl: './generals.component.html',
  styleUrls: ['./generals.component.scss']
})
export class GeneralsComponent implements OnInit {
  generals: General[] = [];
  message = '';
  error = '';
  loading = true;

  // Portrety wg cechy głównej (grafiki wycięte z oryginalnego Red Dragon — generalowie/doradcy).
  private traitPortraits: { [key: string]: string } = {
    'Wodz': 'wodz',
    'Obronca': 'obronca',
    'Mag': 'mag',
    'Zlodziej': 'zlodziej',
    'Kupiec': 'kupiec',
    'Profesor': 'profesor'
  };

  constructor(private generalService: GeneralService, private translate: TranslateService) {}

  ngOnInit(): void {
    this.load();
  }

  portrait(trait: string): string {
    const file = this.traitPortraits[trait] ?? 'wodz';
    return `assets/img/generalowie/${file}.png`;
  }

  load(): void {
    this.generalService.getGenerals().subscribe({
      next: g => { this.generals = g; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  get pending(): General[] {
    return this.generals.filter(g => g.isPending);
  }

  get active(): General[] {
    return this.generals.filter(g => !g.isPending);
  }

  trait(key: string): string {
    const k = `gen.trait.${key}`;
    const t = this.translate.instant(k);
    return t === k ? key : t;
  }

  /** Status generała — tłumaczony, z fallbackiem do wartości z backendu. */
  statusLabel(status: string): string {
    const map: { [k: string]: string } = {
      'W domu': 'gen.status.home',
      'Na wyprawie': 'gen.status.expedition',
      'Ranny': 'gen.status.wounded'
    };
    const k = map[status];
    if (!k) return status;
    const t = this.translate.instant(k);
    return t === k ? status : t;
  }

  accept(general: General): void {
    this.generalService.accept(general.id).subscribe({
      next: r => { this.message = r.message ?? ''; this.error = ''; this.load(); },
      error: e => { this.error = e.error || this.translate.instant('gen.errAccept'); }
    });
  }

  rerollSecondary(general: General): void {
    this.generalService.rerollSecondary(general.id).subscribe({
      next: r => { this.message = r.message ?? ''; this.error = ''; this.load(); },
      error: e => { this.error = e.error || this.translate.instant('gen.errReroll'); }
    });
  }

  reject(general: General): void {
    if (!confirm(this.translate.instant('gen.confirmReject', { trait: this.trait(general.primaryTrait), name: general.name }))) {
      return;
    }
    this.generalService.dismiss(general.id).subscribe({
      next: r => { this.message = this.translate.instant('gen.rejected'); this.error = ''; this.load(); },
      error: e => { this.error = e.error || this.translate.instant('gen.errReject'); }
    });
  }

  dismiss(general: General): void {
    if (!confirm(this.translate.instant('gen.confirmDismiss', { name: general.name, level: general.level }))) {
      return;
    }
    this.generalService.dismiss(general.id).subscribe({
      next: r => { this.message = r.message ?? ''; this.load(); },
      error: e => { this.error = e.error || this.translate.instant('gen.errDismiss'); }
    });
  }
}
