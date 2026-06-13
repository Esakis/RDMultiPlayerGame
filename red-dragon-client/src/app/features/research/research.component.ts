import { Component, OnInit } from '@angular/core';
import { ResearchService } from '../../core/services/research.service';
import { TechDefinition, ResearchStatus } from '../../core/models/kingdom.model';

@Component({
  selector: 'app-research',
  templateUrl: './research.component.html',
  styleUrls: ['./research.component.scss']
})
export class ResearchComponent implements OnInit {
  techDefs: TechDefinition[] = [];
  status: ResearchStatus | null = null;
  loading = true;
  message = '';
  error = '';

  // Kolejność wyświetlania kategorii (pozostałe dołączane na końcu)
  private categoryOrder = ['Nauka', 'Budowa', 'Magia', 'Wojsko', 'Ekonomia', 'Ziemia', 'Czas', 'Smoki'];

  categoryLabels: { [key: string]: string } = {
    Nauka: '🔬 Nauka', Budowa: '🏛️ Budowa', Magia: '🔮 Magia', Wojsko: '⚔️ Wojsko',
    Ekonomia: '💰 Ekonomia', Ziemia: '🌍 Ziemia', Czas: '⏳ Czas', Smoki: '🐉 Smoki'
  };

  constructor(private researchService: ResearchService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.researchService.getAvailableTechnologies().subscribe({
      next: t => { this.techDefs = t; this.loading = false; },
      error: () => { this.loading = false; this.error = 'Błąd wczytywania badań.'; }
    });
    this.researchService.getStatus().subscribe({ next: s => this.status = s, error: () => {} });
  }

  get categories(): string[] {
    const present = Array.from(new Set(this.techDefs.map(t => t.category)));
    const ordered = this.categoryOrder.filter(c => present.includes(c));
    const rest = present.filter(c => !this.categoryOrder.includes(c));
    return [...ordered, ...rest];
  }

  byCategory(cat: string): TechDefinition[] {
    return this.techDefs.filter(t => t.category === cat);
  }

  catLabel(cat: string): string {
    return this.categoryLabels[cat] ?? cat;
  }

  progress(t: TechDefinition): number {
    if (t.costScience <= 0) return 0;
    return Math.min(100, Math.round(t.investedScience / t.costScience * 100));
  }

  invest(t: TechDefinition): void {
    this.message = '';
    this.error = '';
    this.researchService.startResearch(t.techType).subscribe({
      next: res => { this.message = res.message || 'Rozpoczęto rozwój.'; this.load(); },
      error: err => { this.error = err.error?.message || err.error || 'Błąd.'; }
    });
  }
}
