# TODO — uwagi i braki do zrobienia

Stan na 2026-07-01. Uwagi od właściciela projektu + wyniki audytu kodu (backend/frontend).

---

## PLAN REALIZACJI (kolejność prac)

### Etap 1 — pętla walki: przygotowania za dnia, rozstrzygnięcie o 5:00
1. [x] Przebudowa panelu ataków — atak = generał prowadzący + dowolna ilość jednostek;
       atak trafia do kolejki i rozstrzyga się przy przeliczeniu; widok zaplanowanych
       ataków z możliwością odwołania przed 5:00.
2. [x] Raporty z przeliczenia — raport ataków na moje księstwo + widok koalicyjny.
3. [x] Ograniczenia celów — czary ofensywne i złodzieje tylko na cele w stanie
       wojny/zasadzki; biała magia także na własną koalicję; czary tylko za siebie.

### Etap 2 — narzędzia koalicyjne (Imperator / GD)
4. [x] Ataki koalicyjne — Imperator/GD zakładają ataki generałami z dowolnego
       księstwa koalicji; zwykłe konto tylko ze swojego.
5. [x] Tablica ogłoszeń koalicji — wpisy HTML, edycja tylko Imperator/GD, trwałe.
6. [x] Wspólne hasło koalicji — logowanie hasłem oryginalnym albo wspólnym.

### Etap 3 — martwe mechaniki z audytu (balans)
7. [x] Bonusy produkcyjne generałów (Kupiec, Profesor, Mag, Złodziej).
8. [x] Zaklęcie Chochliki (MachineDamage) + realne Upijanie armii.
9. [x] Ekonomia wojenna: Renowacja broni, Port towarowy, machiny Goblina w wieżach.

### Etap 4 — widoczność i porządki
10. [x] Badge nieprzeczytanych wiadomości, sygnał po przeliczeniu, odliczanie do 5:00.
11. [x] Endpoint i widok Panteonu + porządny ranking.
12. [x] Nawigacja: podpiąć /research i /dragons, usunąć martwe linki, scalić menu.

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
- ✅ **B1. Bonusy produkcyjne generałów** — ZROBIONE: Kupiec (+1,5·lvl/(lvl+50) kupcom),
  Profesor (+lvl p.p. szkolenia), Mag i Złodziej (+lvl/(lvl+50) siły) — liczy się
  najlepszy generał w domu.
- ✅ **B2. Zaklęcie Chochliki** — ZROBIONE: niszczy 10–20% machin (Gnom odporny,
  machiny Goblina niezniszczalne).
- ✅ **B3. Upijanie armii** — ZROBIONE: `Kingdom.DrunkArmyPct` obniża obronę o 25%
  w fazie wojskowej tego samego przeliczenia.

### Ważne
- ✅ **B4. Renowacja broni** — ZROBIONE: +5 broni za własnego poległego,
  w wojnie koalicji 40–50 tys. broni/przeliczenie.
- ✅ **B5. Port towarowy** — ZROBIONE: nowy budynek specjalny (rząd 5),
  400–600 tys. złota/turę, w wojnie ×2.
- ✅ **B6. Goblińska inżynieria obronna** — ZROBIONE: wieże Goblina mieszczą
  po 10 machin broniących z siłą 100.
- ✅ **B7. Cechy generałów w bitwie** — ZROBIONE: Porwanie (2·lvl%) i Zabójstwo (2·lvl%)
  po zwycięstwie, Zranienie (3·lvl%, na 3 dni) także przy porażce.
- ✅ **B8. Panteon** — ZROBIONE: `GET /api/coalition/pantheon` + zakładka
  Panteon w Statystykach.

### Drobne / kalibracja
- ✅ **B9.** ZROBIONE: plagi losowe przy przeliczeniu (Zaraza 3%, Szarańcza 3%,
  Chochliki 2% — z odpornościami rasowymi) oraz auto-rzucanie wybranego pozytywnego
  zaklęcia na siebie po przeliczeniu (`Kingdom.AutoCastSpellType`).
  Auto-SELL many jest bezprzedmiotowe — w tym modelu mana nie znika po turze,
  tylko dąży do pojemności wyznaczanej przez druidów (świadoma decyzja projektowa).
- ⏸ **B10.** Bazowe produkcje profesji (`ResourceService.cs:16-24`) — wartości
  przybliżone z zachowanymi proporcjami; oryginalny manual nie podaje pełnych baz
  (docs/MECHANIKA.md §15). Kalibrować dopiero na podstawie testów rozgrywki.
- ⏸ **B11.** Próg głodu 5% i tempo expa generałów — jw.: brak danych źródłowych,
  wartości działają; do strojenia po testach z graczami.
- ✅ **B12.** Nieaktualny komentarz w `DailyResetService` — usunięty.

---

## C. Wyniki audytu — frontend (red-dragon-client)

- ✅ **C1. Brak odświeżania danych** — ZROBIONE: `GET /api/notification/status`
  + polling co 60 s, badge poczty i raportów w nagłówku, licznik do 5:00.
- ✅ **C2. Trasy osierocone** — ZROBIONE: Nauka i Smoki w menu bocznym.
- ✅ **C3. Martwe linki w headerze** — ZROBIONE: usunięte (Wieści/Doradcy/Czat),
  w zamian link Raporty z badge.
- ✅ **C4. Nawigacja** — uporządkowana: sidebar ma wszystkie widoki gry, górny pasek
  to skróty (Szczegóły/Forum/Poczta/Raporty/Labirynt) + licznik przeliczenia.
- ✅ **C5. Widok Smoków** — ZROBIONE: stan budynków smoczych, tempo wabienia,
  orientacyjny koszt przywołania, przywołanie bezpośrednio z widoku.
