import { Component, OnInit } from '@angular/core';
import { ResearchService } from '../../core/services/research.service';
import { TechDefinition, Research } from '../../core/models/kingdom.model';

@Component({
  selector: 'app-research',
  templateUrl: './research.component.html',
  styleUrls: ['./research.component.scss']
})
export class ResearchComponent implements OnInit {
  techDefs: TechDefinition[] = [];
  myResearch: Research[] = [];
  loading = true;
  message = '';
  categories = ['Economy', 'Military', 'Magic', 'Thieves'];

  constructor(private researchService: ResearchService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.researchService.getAvailableTechnologies().subscribe(t => this.techDefs = t);
    this.researchService.getMyResearch().subscribe(r => { this.myResearch = r; this.loading = false; });
  }

  getByCategory(cat: string): TechDefinition[] {
    return this.techDefs.filter(t => t.category === cat);
  }

  getResearchStatus(techType: string): Research | undefined {
    return this.myResearch.find(r => r.techType === techType);
  }

  startResearch(techType: string): void {
    this.researchService.startResearch(techType).subscribe({
      next: (res) => { this.message = res.message || 'Badanie rozpoczęte.'; this.load(); setTimeout(() => this.message = '', 4000); },
      error: (err) => { this.message = err.error?.message || err.error || 'Błąd'; setTimeout(() => this.message = '', 4000); }
    });
  }

  getCatName(cat: string): string {
    return { 'Economy': '💰 Ekonomia', 'Military': '⚔️ Wojsko', 'Magic': '🔮 Magia', 'Thieves': '🗡️ Złodzieje' }[cat] || cat;
  }
}
