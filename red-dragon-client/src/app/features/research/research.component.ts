import { Component, OnInit } from '@angular/core';
import { ResearchService } from '../../core/services/research.service';
import { TechDefinition, ResearchStatus } from '../../core/models/kingdom.model';
import { CATEGORY_ICONS, techIconPath } from '../../shared/tech-icons';

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

  // Dziedzina oczekująca na potwierdzenie uruchomienia (popup).
  confirmTech: TechDefinition | null = null;

  // Kolejność wyświetlania kategorii (pozostałe dołączane na końcu)
  private categoryOrder = ['Nauka', 'Budowa', 'Magia', 'Wojsko', 'Ekonomia', 'Ziemia', 'Czas', 'Smoki'];

  categoryLabels: { [key: string]: string } = {
    Nauka: 'Nauka', Budowa: 'Budowa', Magia: 'Magia', Wojsko: 'Wojsko',
    Ekonomia: 'Ekonomia', Ziemia: 'Ziemia', Czas: 'Czas', Smoki: 'Smoki'
  };

  // Ikony kategorii / dziedzin wycięte z oryginalnego drzewka — wspólne źródło: shared/tech-icons.
  private categoryIcons = CATEGORY_ICONS;

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

  /**
   * Łańcuchy badań jako kolumny drzewka (jak w oryginalnym „drzewie nauki"):
   * korzeń = dziedzina bez wymagań, kolejne ogniwa wg requiredTech. Kolumny
   * posortowane wg kolejności kategorii.
   */
  get chains(): TechDefinition[][] {
    const nextOf = new Map<string, TechDefinition>();
    for (const t of this.techDefs) {
      if (t.requiredTech) nextOf.set(t.requiredTech, t);
    }
    const roots = this.techDefs.filter(t => !t.requiredTech);
    const chains = roots.map(root => {
      const chain = [root];
      let cur = root;
      while (nextOf.has(cur.techType)) {
        cur = nextOf.get(cur.techType)!;
        chain.push(cur);
      }
      return chain;
    });

    // Wizualne złączenie w jednej kolumnie BEZ zależności badawczej
    // (Konstrukcja maszyn pokazywana pod Empiryzmem, ale odkrywalna niezależnie).
    const byRoot = new Map(chains.map(c => [c[0].techType, c]));
    for (const [child, parent] of Object.entries(this.visualParent)) {
      const cc = byRoot.get(child), pc = byRoot.get(parent);
      if (cc && pc) { pc.push(...cc); byRoot.delete(child); }
    }
    const merged = Array.from(byRoot.values());

    const catIdx = (c: string) => {
      const i = this.categoryOrder.indexOf(c);
      return i < 0 ? 99 : i;
    };
    return merged.sort((a, b) =>
      catIdx(a[0].category) - catIdx(b[0].category) || b.length - a.length);
  }

  /** Dziedziny pokazywane w kolumnie pod inną, lecz odkrywane niezależnie (bez strzałki). */
  private visualParent: { [child: string]: string } = { KonstrukcjaMaszyn: 'Empiryzm' };

  /** Czy między kafelkiem a poprzednim w kolumnie istnieje realna zależność (strzałka). */
  isDependent(chain: TechDefinition[], i: number): boolean {
    return i > 0 && chain[i].requiredTech === chain[i - 1].techType;
  }

  /** Krótsze dziedziny/specjalizacje — wyświetlane na górze (jak w oryginale). */
  get specChains(): TechDefinition[][] {
    return this.chains.filter(c => c.length < 5);
  }

  /** Sześć głównych łańcuchów 5-poziomowych — wyświetlane niżej jako kolumny. */
  get mainChains(): TechDefinition[][] {
    return this.chains.filter(c => c.length >= 5);
  }

  catLabel(cat: string): string {
    return this.categoryLabels[cat] ?? cat;
  }

  catIcon(cat: string): string | null {
    const file = this.categoryIcons[cat];
    return file ? `assets/img/nauka/${file}.png` : null;
  }

  techIcon(t: TechDefinition): string {
    return techIconPath(t.techType, t.category);
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

  // Klik na kafelek: tylko dostępną dziedzinę można uruchomić — otwórz popup.
  onTileClick(t: TechDefinition): void {
    if (!t.isCompleted && !t.isCurrent && t.canResearch) {
      this.confirmTech = t;
    }
  }

  confirmStart(): void {
    if (this.confirmTech) { this.invest(this.confirmTech); }
    this.confirmTech = null;
  }

  cancelConfirm(): void {
    this.confirmTech = null;
  }
}
