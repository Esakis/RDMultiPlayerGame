import { Component, OnInit } from '@angular/core';
import { BuildingService } from '../../core/services/building.service';
import { BuildingDefinition, Building } from '../../core/models/kingdom.model';

type SpecialState = 'owned' | 'current' | 'available' | 'locked';

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

  // Etykiety polskich kategorii (zgodne z danymi z backendu)
  private categoryLabels: { [k: string]: string } = {
    Gospodarcze: '🏠 Gospodarcze', Warsztaty: '🔨 Warsztaty', Cechy: '⭐ Cechy',
    Manufaktury: '🏭 Manufaktury', Pozostale: '📦 Pozostałe', Obrona: '🛡️ Obrona',
    Wojskowe: '⚔️ Wojskowe', Specjalne: '✨ Specjalne'
  };

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

  /** Zwykłe kategorie (bez specjalnych), w kolejności wystąpienia w danych. */
  get normalCategories(): string[] {
    return Array.from(new Set(this.definitions.filter(d => !d.isSpecial).map(d => d.category)));
  }

  getByCategory(cat: string): BuildingDefinition[] {
    return this.definitions.filter(d => d.category === cat && !d.isSpecial);
  }

  getOwned(type: string): Building | undefined {
    return this.myBuildings.find(b => b.buildingType === type);
  }

  construct(def: BuildingDefinition): void {
    const qty = def.isSpecial ? 1 : (this.quantities[def.buildingType] || 1);
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
    return this.categoryLabels[cat] || cat;
  }

  // ===== Drzewko budynków specjalnych =====

  /**
   * Łańcuchy budynków specjalnych jako kolumny (jak w drzewku nauki):
   * 6 kolumn, w każdej budynki posortowane wg rzędu (rosnący koszt bazowy).
   * Zależność między kafelkami wynika z requiredBuildingType (strzałka w dół).
   */
  get specialChains(): BuildingDefinition[][] {
    const specials = this.definitions.filter(d => d.isSpecial);
    const cols = Array.from(new Set(specials.map(d => d.col))).sort((a, b) => a - b);
    return cols.map(c =>
      specials.filter(d => d.col === c).sort((a, b) => a.row - b.row));
  }

  get hasSpecial(): boolean {
    return this.definitions.some(d => d.isSpecial);
  }

  /** Typ budynku specjalnego aktualnie wznoszonego (jeden naraz) lub null. */
  get specialUnderConstruction(): string | null {
    const specialTypes = this.definitions.filter(d => d.isSpecial).map(d => d.buildingType);
    const b = this.myBuildings.find(x => x.isUnderConstruction && specialTypes.includes(x.buildingType));
    return b ? b.buildingType : null;
  }

  /** Stan kafelka budynku specjalnego. */
  specialState(def: BuildingDefinition): SpecialState {
    const owned = this.getOwned(def.buildingType);
    if (owned?.isUnderConstruction) return 'current';
    if (owned && owned.quantity > 0) return 'owned';
    // Tylko jeden budynek specjalny może powstawać jednocześnie.
    const inProgress = this.specialUnderConstruction;
    if (inProgress && inProgress !== def.buildingType) return 'locked';
    if (def.canBuild) return 'available';
    return 'locked';
  }

  /** Powód, dla którego budynek specjalny jest niedostępny. */
  specialLockReason(def: BuildingDefinition): string {
    const inProgress = this.specialUnderConstruction;
    if (inProgress && inProgress !== def.buildingType)
      return 'Najpierw dokończ obecnie wznoszony budynek specjalny.';
    return def.cannotBuildReason || 'Wymaga wcześniejszego budynku';
  }

  /** Czy między kafelkiem a poprzednim w kolumnie istnieje realna zależność (strzałka). */
  isDependent(chain: BuildingDefinition[], i: number): boolean {
    return i > 0 && !!chain[i].requiredBuildingType
      && chain[i].requiredBuildingType === chain[i - 1].buildingType;
  }

  /** Klik na kafelek: zbuduj tylko dostępny budynek specjalny. */
  onSpecialClick(def: BuildingDefinition): void {
    if (this.specialState(def) === 'available') {
      this.construct(def);
    }
  }
}
