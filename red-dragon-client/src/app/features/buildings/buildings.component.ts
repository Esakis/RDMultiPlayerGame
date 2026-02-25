import { Component, OnInit } from '@angular/core';
import { BuildingService } from '../../core/services/building.service';
import { BuildingDefinition, Building } from '../../core/models/kingdom.model';

@Component({
  selector: 'app-buildings',
  templateUrl: './buildings.component.html',
  styleUrls: ['./buildings.component.scss']
})
export class BuildingsComponent implements OnInit {
  definitions: BuildingDefinition[] = [];
  myBuildings: Building[] = [];
  loading = true;
  message = '';
  quantities: { [key: string]: number } = {};

  categories = ['Residential', 'Economic', 'Scientific', 'Military', 'Defensive', 'Special'];

  constructor(private buildingService: BuildingService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.buildingService.getAvailableBuildings().subscribe(defs => {
      this.definitions = defs;
      defs.forEach(d => { if (!this.quantities[d.buildingType]) this.quantities[d.buildingType] = 1; });
    });
    this.buildingService.getMyBuildings().subscribe(b => {
      this.myBuildings = b;
      this.loading = false;
    });
  }

  getByCategory(cat: string): BuildingDefinition[] {
    return this.definitions.filter(d => d.category === cat);
  }

  getOwned(type: string): Building | undefined {
    return this.myBuildings.find(b => b.buildingType === type);
  }

  construct(def: BuildingDefinition): void {
    const qty = this.quantities[def.buildingType] || 1;
    this.buildingService.construct({ buildingType: def.buildingType, quantity: qty }).subscribe({
      next: (res) => {
        this.message = res.message || 'Budowa rozpoczęta.';
        this.load();
        setTimeout(() => this.message = '', 4000);
      },
      error: (err) => {
        this.message = err.error?.message || err.error || 'Błąd budowy.';
        setTimeout(() => this.message = '', 4000);
      }
    });
  }

  getCategoryName(cat: string): string {
    const map: {[k:string]:string} = {
      'Residential': '🏠 Mieszkalne', 'Economic': '💰 Ekonomiczne', 'Scientific': '📚 Naukowe',
      'Military': '⚔️ Wojskowe', 'Defensive': '🛡️ Obronne', 'Special': '⭐ Specjalne'
    };
    return map[cat] || cat;
  }
}
