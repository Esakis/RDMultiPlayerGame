# TODO — uwagi i braki do zrobienia

Stan na 2026-07-01. Uwagi od właściciela projektu + wyniki audytu kodu (backend/frontend).

---

## PLAN REALIZACJI (kolejność prac)

### Etap 1 — pętla walki: przygotowania za dnia, rozstrzygnięcie o 5:00
1. [ ] Przebudowa panelu ataków — atak = generał prowadzący + dowolna ilość jednostek;
       atak trafia do kolejki i rozstrzyga się przy przeliczeniu; widok zaplanowanych
       ataków z możliwością odwołania przed 5:00.
2. [ ] Raporty z przeliczenia — raport ataków na moje księstwo + widok koalicyjny.
3. [ ] Ograniczenia celów — czary ofensywne i złodzieje tylko na cele w stanie
       wojny/zasadzki; biała magia także na własną koalicję; czary tylko za siebie.

### Etap 2 — narzędzia koalicyjne (Imperator / GD)
4. [ ] Ataki koalicyjne — Imperator/GD zakładają ataki generałami z dowolnego
       księstwa koalicji; zwykłe konto tylko ze swojego.
5. [ ] Tablica ogłoszeń koalicji — wpisy HTML, edycja tylko Imperator/GD, trwałe.
6. [ ] Wspólne hasło koalicji — logowanie hasłem oryginalnym albo wspólnym.

### Etap 3 — martwe mechaniki z audytu (balans)
7. [ ] Bonusy produkcyjne generałów (Kupiec, Profesor, Mag, Złodziej).
8. [ ] Zaklęcie Chochliki (MachineDamage) + realne Upijanie armii.
9. [ ] Ekonomia wojenna: Renowacja broni, Port towarowy, machiny Goblina w wieżach.

### Etap 4 — widoczność i porządki
10. [ ] Badge nieprzeczytanych wiadomości, sygnał po przeliczeniu, odliczanie do 5:00.
11. [ ] Endpoint i widok Panteonu + porządny ranking.
12. [ ] Nawigacja: podpiąć /research i /dragons, usunąć martwe linki, scalić menu.

---

## A. Uwagi od właściciela (priorytet — model rozgrywki)

### A1. Rytm gry: dzień przygotowań → walki raz dziennie o 5:00
- Każde konto ma dzienną pulę **tur**, którymi w ciągu dnia wykonuje akcje:
  rozbudowę, ekonomię, rekrutację — czyli **przygotowuje księstwo i koalicję do walki**.
- **Walki NIE odbywają się natychmiast ani „na tury"** — ataki założone w ciągu dnia
  rozstrzygają się **wyłącznie raz dziennie przy przeliczeniu o 5:00** (faza wojskowa).
  Potem znów cały dzień przygotowań i kolejne przeliczenie następnego dnia o 5:00.
- Po przeliczeniu gracz musi dostać **raport z ataków**:
  - ataki na jego księstwo (kto, czym, wynik, straty, zdobycz),
  - ataki na księstwa jego koalicji (widok koalicyjny).

### A2. Panel wysyłania ataków — do przebudowy (obecnie nieobsługiwalny)
- Założenie ataku wojskowego = wybór **generała** prowadzącego atak
  + dołączenie **dowolnej ilości wojska** (jednostki wybierane ręcznie).
- Zwykłe konto może zakładać ataki **tylko ze swojego księstwa**.
- **Imperator oraz GD (Głównodowodzący)** mogą zakładać ataki **generałami
  z dowolnego księstwa swojej koalicji** (panel koalicyjny do planowania ataków).

### A3. Czary i złodzieje — ograniczenia celu
- Czary rzuca **każde księstwo osobno** (nie ma rzucania za kogoś).
- Czary ofensywne można rzucać **tylko na księstwo, któremu wypowiedziano wojnę
  lub zasadzkę** (stan wojny/przepadu między koalicjami).
- **To samo dotyczy akcji złodziejskich** — tylko cele w stanie wojny/zasadzki.
- **Białą magię** (pozytywną) można rzucać także **na księstwa własnej koalicji**.

### A4. Wspólne hasło koalicji
- Imperator oraz GD mogą ustawić **wspólne hasło koalicji**.
- Na każde księstwo koalicji można się zalogować:
  - loginem + hasłem oryginalnym (właściciel), **albo**
  - loginem + hasłem wspólnym koalicji.
- Hasło wspólne przestaje działać po opuszczeniu koalicji / zmianie hasła wspólnego.

### A5. Tablica ogłoszeń koalicji
- Nowy moduł: **tablica ogłoszeń** widoczna dla całej koalicji.
- Edytować (dodawać/zmieniać/usuwać wpisy) mogą **tylko Imperator i GD**.
- Treść ogłoszeń w **HTML** (formatowanie dowolne).
- Ogłoszenia **nigdy nie znikają same** — tylko ręczne usunięcie.

---

## B. Wyniki audytu — backend (RedDragonAPI)

### Krytyczne (psują balans)
- **B1. Bonusy produkcyjne generałów martwe** — Kupiec (+złoto kupców),
  Profesor (+szkolenie nowicjuszy), Mag (+siła zaklęć), Złodziej (+siła złodziei)
  zbierają expy, ale bonusy nie są nigdzie stosowane (brak odwołań w `ResourceService`).
- **B2. Zaklęcie Chochliki bez efektu** — `EffectType="MachineDamage"` nie ma obsługi
  w `ApplySpellEffect` (`BattleService`), czar wisi i nic nie robi.
- **B3. Upijanie armii kosmetyczne** — akcja złodziejska zwraca tylko tekst,
  nie osłabia obrony celu przy najbliższym przeliczeniu.

### Ważne
- **B4. Renowacja broni bez efektu** — budynek jest w seedzie, brak logiki
  (broń za poległych, produkcja broni w wojnie).
- **B5. Port towarowy nie istnieje** — wg manuala PL: 400–600k złota/turę, w wojnie ×2.
- **B6. Goblińska inżynieria obronna** — machiny Goblina nie bronią w wieżach
  (TODO w `BattleCalculator.cs:85`).
- **B7. Cechy generałów w bitwie** — Porwanie/Zabójstwo/Zranienie generała działają
  tylko jako akcje złodziei; w starciu zbrojnym nieaktywne.
- **B8. Panteon niewidoczny** — `EraConcluder` zapisuje zwycięzców do encji `Pantheon`,
  ale żaden endpoint jej nie czyta; brak też dedykowanego endpointu rankingowego.

### Drobne / kalibracja
- **B9.** Auto-cast / auto-sell i zdarzenia losowe (plagi) z listy tury — brak.
- **B10.** Bazowe produkcje profesji przybliżone (`ResourceService.cs:16-24`) — do kalibracji.
- **B11.** Zgadywane stałe: próg głodu 5%, tempo expa generałów — do weryfikacji z manualem.
- **B12.** Nieaktualny komentarz TODO w `DailyResetService.cs:232` (funkcja już jest niżej).

---

## C. Wyniki audytu — frontend (red-dragon-client)

- **C1. Brak odświeżania danych** — zero pollingu; gracz nie widzi nowej wiadomości
  ani wyników przeliczenia bez ręcznego odświeżenia. Potrzebne: badge nieprzeczytanych
  wiadomości, sygnał „było przeliczenie — zobacz raporty", odliczanie do 5:00.
- **C2. Trasy osierocone** — `/research` i `/dragons` istnieją w routingu,
  ale nie prowadzi do nich żaden link w menu.
- **C3. Martwe linki w headerze** — Aktualności / Doradcy / Czat to `javascript:void(0)`.
- **C4. Nawigacja rozbita** — sidebar (12 pozycji) vs górny pasek (forum, wiadomości,
  labirynt) — do ujednolicenia.
- **C5. Widok Smoków ubogi** — tylko podgląd, akcja deleguje do `/magic`.
