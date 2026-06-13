import { Component, OnInit } from '@angular/core';
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

  traitNames: { [key: string]: string } = {
    'Wodz': 'Wódz',
    'Obronca': 'Obrońca',
    'Mag': 'Mag',
    'Zlodziej': 'Złodziej',
    'Kupiec': 'Kupiec',
    'Profesor': 'Profesor',
    'PorwanieGenerala': 'Porwanie generała',
    'ZabojstwoGenerala': 'Zabójstwo generała',
    'ZranienieGenerala': 'Zranienie generała',
    'Smokobojstwo': 'Smokobójstwo',
    'Uzdrawianie': 'Uzdrawianie',
    'Sabotaz': 'Sabotaż',
    'Krwiozerczonsc': 'Krwiożerczość',
    'Rabunek': 'Rabunek',
    'MaskowanieISzpiegostwo': 'Maskowanie i szpiegostwo',
    'CzarnaMagia': 'Czarna magia',
    'MagiaCzasu': 'Magia czasu',
    'BialaMagia': 'Biała magia'
  };

  constructor(private generalService: GeneralService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.generalService.getGenerals().subscribe({
      next: g => { this.generals = g; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  trait(key: string): string {
    return this.traitNames[key] ?? key;
  }

  dismiss(general: General): void {
    if (!confirm(`Czy na pewno zwolnić generała ${general.name} (poziom ${general.level})? Tego nie można cofnąć.`)) {
      return;
    }
    this.generalService.dismiss(general.id).subscribe({
      next: r => { this.message = r.message ?? ''; this.load(); },
      error: e => { this.error = e.error || 'Błąd zwalniania generała.'; }
    });
  }
}
