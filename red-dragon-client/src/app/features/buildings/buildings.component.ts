import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { BuildingService } from '../../core/services/building.service';
import { KingdomService } from '../../core/services/kingdom.service';
import { BuildingDefinition, Building, Kingdom } from '../../core/models/kingdom.model';

type SpecialState = 'owned' | 'current' | 'available' | 'locked';
type BuildTab = 'gospodarcze' | 'specjalne' | 'ziemia' | 'nauka';

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

  // Aktywna zakładka u góry sekcji Budowa.
  activeTab: BuildTab = 'gospodarcze';

  // Dane do panelu „Ziemia" (kupno ziemi przeniesione z Zatrudnienia).
  kingdom: Kingdom | null = null;
  landAmount = 1;

  // Etykiety polskich kategorii (zgodne z danymi z backendu)
  // Ikony kategorii (etykieta tekstowa pochodzi z tłumaczeń bld.cat.*).
  private categoryIcons: { [k: string]: string } = {
    Gospodarcze: '🏠', Warsztaty: '🔨', Cechy: '⭐',
    Manufaktury: '🏭', Pozostale: '📦', Obrona: '🛡️',
    Wojskowe: '⚔️', Specjalne: '✨'
  };

  constructor(
    private buildingService: BuildingService,
    private kingdomService: KingdomService,
    private translate: TranslateService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    // Otwórz właściwą zakładkę, gdy przyjdziemy z kafelka na Stolicy (?tab=specjalne|nauka).
    const tab = this.route.snapshot.queryParamMap.get('tab') as BuildTab | null;
    if (tab && ['gospodarcze', 'specjalne', 'ziemia', 'nauka'].includes(tab)) {
      this.activeTab = tab;
    }
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
    this.kingdomService.getMyKingdom().subscribe(k => this.kingdom = k);
  }

  setTab(tab: BuildTab): void {
    this.activeTab = tab;
  }

  buyLand(): void {
    if (this.landAmount <= 0) return;
    this.kingdomService.buyLand(this.landAmount).subscribe({
      next: (res) => {
        this.message = res.message || 'Zakupiono ziemię.';
        this.kingdomService.getMyKingdom().subscribe(k => this.kingdom = k);
        setTimeout(() => this.message = '', 4000);
      },
      error: (err) => {
        this.message = err.error?.message || err.error || 'Błąd zakupu ziemi.';
        setTimeout(() => this.message = '', 4000);
      }
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

  demolish(def: BuildingDefinition): void {
    const owned = this.getOwned(def.buildingType);
    if (!owned || owned.quantity <= 0) {
      this.message = this.translate.instant('bld.nothingToDemolish');
      setTimeout(() => this.message = '', 4000);
      return;
    }
    const qty = this.quantities[def.buildingType] || 1;
    this.buildingService.demolish({ buildingType: def.buildingType, quantity: qty }).subscribe({
      next: (res) => {
        this.message = res.message || 'Wyburzono.';
        this.load();
        setTimeout(() => this.message = '', 4000);
      },
      error: (err) => {
        this.message = err.error?.message || err.error || 'Błąd wyburzania.';
        setTimeout(() => this.message = '', 4000);
      }
    });
  }

  getCategoryName(cat: string): string {
    const icon = this.categoryIcons[cat];
    const key = `bld.cat.${cat}`;
    const label = this.translate.instant(key);
    const text = label === key ? cat : label;
    return icon ? `${icon} ${text}` : text;
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
      return this.translate.instant('bld.finishCurrentFirst');
    return def.cannotBuildReason || this.translate.instant('bld.requiresPrevious');
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
