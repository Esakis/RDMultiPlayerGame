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

  // Dziedzina oczekująca na potwierdzenie uruchomienia (popup).
  confirmTech: TechDefinition | null = null;

  // Kolejność wyświetlania kategorii (pozostałe dołączane na końcu)
  private categoryOrder = ['Nauka', 'Budowa', 'Magia', 'Wojsko', 'Ekonomia', 'Ziemia', 'Czas', 'Smoki'];

  categoryLabels: { [key: string]: string } = {
    Nauka: 'Nauka', Budowa: 'Budowa', Magia: 'Magia', Wojsko: 'Wojsko',
    Ekonomia: 'Ekonomia', Ziemia: 'Ziemia', Czas: 'Czas', Smoki: 'Smoki'
  };

  // Ikony kategorii wycięte z drzewka „Specjalizacja księstwa" oryginalnego Red Dragon.
  private categoryIcons: { [key: string]: string } = {
    Nauka: 'nauka', Budowa: 'budowa', Magia: 'magia', Wojsko: 'wojsko',
    Ekonomia: 'ekonomia', Ziemia: 'ziemia', Czas: 'czas', Smoki: 'smoki'
  };

  // Ikona per dziedzina — pozycja kafelka (r{wiersz}c{kolumna}) z oryginalnego drzewka nauki.
  // Dolne 5 kolumn = łańcuchy 5-poziomowe; górne wiersze = pojedyncze specjalizacje.
  // Każde badanie ma własny kafelek z oryginalnego drzewka (nauka.png, 6 kolumn × 8 wierszy).
  // Łańcuchy 5-poziomowe = 6 kolumn (c0..c5) × wiersze r3..r7; specjalizacje = górne wiersze r0..r2.
  private techIcons: { [key: string]: string } = {
    // Nauka — Wynalazczość (kolumna 0)
    Empiryzm: 'r1c0',
    Wynalazki1: 'r3c0', Wynalazki2: 'r4c0', Wynalazki3: 'r5c0', Wynalazki4: 'r6c0', Wynalazki5: 'r7c0',
    // Budowa — Architektura (kol. 1), Inżynieria (kol. 2)
    Architektura1: 'r3c1', Architektura2: 'r4c1', Architektura3: 'r5c1', Architektura4: 'r6c1', Architektura5: 'r7c1',
    Inzynieria1: 'r3c2', Inzynieria2: 'r4c2', Inzynieria3: 'r5c2', Inzynieria4: 'r6c2', Inzynieria5: 'r7c2',
    // Magia — Czarodziejstwo (kol. 3)
    Czarodziejstwo1: 'r3c3', Czarodziejstwo2: 'r4c3', Czarodziejstwo3: 'r5c3', Czarodziejstwo4: 'r6c3', Czarodziejstwo5: 'r7c3',
    // Wojsko — Trening (kol. 4), Rekrutacja (kol. 5)
    Trening1: 'r3c4', Trening2: 'r4c4', Trening3: 'r5c4', Trening4: 'r6c4', Trening5: 'r7c4',
    Rekrutacja1: 'r3c5', Rekrutacja2: 'r4c5', Rekrutacja3: 'r5c5', Rekrutacja4: 'r6c5', Rekrutacja5: 'r7c5',
    // Wojsko — broń (specjalizacje)
    OstrzenieBroni: 'r1c4', NaprawaBroni: 'r2c0', PrzekuwanieBroni: 'r1c4',
    // Czas
    ZakrzywCzasu: 'r0c1', ZalamCzasu: 'r1c1',
    // Ziemia
    Osadnictwo: 'r0c2', Rekultywacja: 'r1c2', GornictwoOdkrywkowe: 'r0c0',
    // Smoki
    Smokoastronomia: 'r0c3', Smokoanatomia: 'r2c3', Smokodynamika: 'r1c3',
    // Ekonomia
    KonstrukcjaMaszyn: 'r0c5', Rachunkowosc: 'r0c4', Buchalteria: 'r2c4', Ksiegowosc: 'r0c4'
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
    const file = this.techIcons[t.techType];
    if (file) return `assets/img/nauka/tech/${file}.png`;
    const cat = this.categoryIcons[t.category];
    return cat ? `assets/img/nauka/${cat}.png` : 'assets/img/nauka/tech/r3c1.png';
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
