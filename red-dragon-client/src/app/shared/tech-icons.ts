// Mapowanie dziedzin nauki na grafiki wycięte z oryginalnego drzewka Red Dragon.
// Wspólne źródło dla zakładki Nauka i podglądu w Stolicy.

/** Ikona kategorii (assets/img/nauka/{plik}.png). */
export const CATEGORY_ICONS: { [key: string]: string } = {
  Nauka: 'nauka', Budowa: 'budowa', Magia: 'magia', Wojsko: 'wojsko',
  Ekonomia: 'ekonomia', Ziemia: 'ziemia', Czas: 'czas', Smoki: 'smoki'
};

// Ikona per dziedzina — pozycja kafelka (r{wiersz}c{kolumna}) z oryginalnego drzewka nauki
// (nauka.png, 6 kolumn × 8 wierszy). Łańcuchy 5-poziomowe = c0..c5 × r3..r7; specjalizacje = r0..r2.
export const TECH_ICON_TILES: { [key: string]: string } = {
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

/** Ścieżka grafiki dla dziedziny nauki — kafelek wg techType, awaryjnie ikona kategorii. */
export function techIconPath(techType: string | null | undefined, category?: string | null): string {
  if (techType && TECH_ICON_TILES[techType]) {
    return `assets/img/nauka/tech/${TECH_ICON_TILES[techType]}.png`;
  }
  if (category && CATEGORY_ICONS[category]) {
    return `assets/img/nauka/${CATEGORY_ICONS[category]}.png`;
  }
  return 'assets/img/nauka/nauka.png';
}
