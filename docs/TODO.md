# TODO — uwagi i braki do zrobienia

Stan na 2026-07-01. Uwagi od właściciela projektu + wyniki audytu kodu (backend/frontend).

---

## PLAN 2026-07-10 — audyt Dracopedia vs aplikacja (runda 2)

Świeży audyt źródeł (docs/zrodla/dracopedia, dracopedia_budynki) vs kod.
Zweryfikowane w kodzie: wszystkie 22 nauki są w grze (koszty SP i drzewko zgodne),
większość budynków specjalnych działa. Poniżej realne braki, wg kolejności prac.
Dracopedia = źródło nadrzędne; przy rozbieżnościach przyjmujemy wartości źródłowe.

### Etap 0 — brakujące źródła
- [x] Pobrać z Dracopedii (Wayback): strony o miesiącach/fazach księżyca
      (Pełnia, Krwawy Księżyc, Złoty Sierp, Pęknięta Tarcza), kategorie Czary,
      Akcje złodziejskie, Pojęcia. Zapisać w docs/zrodla, uzupełnić MECHANIKA.md.
      → 19 stron w docs/zrodla/dracopedia_pojecia/ (m.in. fazy_ksiezyca, wzory,
      rozpiska_magiczna, zawody); „Karawany" i „Wzmacniacze magii" nie były
      zarchiwizowane (404). Fazy księżyca opisane w MECHANIKA.md §13.

### Etap 1 — system miesięcy (faz księżyca) — BRAK CAŁKOWITY
- [x] Cykl faz przy przeliczeniu dziennym + wskaźnik fazy w UI.
      → MoonPhaseHelper (GameSettings: MoonPhase/MoonBloodMoon), przesuw w
      DailyResetService, wskaźnik 🌙 w nagłówku (polling /notification/status).
- [x] Efekty bazowe wg fazy_ksiezyca.html: Nów (akcje złodziejskie ujawniają 1–6
      paktów celu), Złoty Sierp (Kopalnia złota ×2), Oko Smoka (smoki z wabienia
      i labiryntu ×2), Garb Autora (łupy z labiryntu −50%), Pęknięta Tarcza
      (Ambulatorium −20% zamiast −50%, Renowacja broni ¼), Krwawy Księżyc
      (losowo zamiast Pełni, awans E1→E2 ×2). Br-Oug odporny poza Nowiem
      i Krwawym Księżycem. Efekty magiczne Pełni (Lustro/Ściany) — w Etapie 2.

### Etap 2 — obrona magiczna z budynków — BRAK
- [x] Ściany magiczne: +15% obrony magicznej.
- [x] Lustro magiczne: +12% obrony magicznej, 25% szansy odbicia czaru (Dżin 50%);
      w Pełni 50%/75% odbicia, bez obrony magicznej; nie dotyczy Olbrzymów.
      Wpiąć w pojedynek magiczny (BattleService).
      → ExecuteSpellAsync: odbicie stosuje efekt czaru na rzucającym (raport
      "Reflected"); tylko czary z TargetType=Enemy. Uwaga: akcja „Sabotaż budynku
      specjalnego" nie istnieje w grze (Burzenie niszczy tylko infrastrukturę),
      więc ochrona Ścian w Pełni jest bezprzedmiotowa.

### Etap 3 — Szpital: redukcja strat w obronie — BRAK
- [x] Szpital: −25% strat wojska w obronie (nie działa na straty od gnomich saperów E2).
      Atakową połowę (−50%) ma Ambulatorium polowe — uporządkować podział ról.
      → BattleService: obrońca ze Szpitalem traci 25% mniej wojska; straty od
      saperów Gnoma doliczane atakującemu po redukcjach (wyjątek zachowany).
      Podział ról zostaje: Szpital=obrona, Ambulatorium=atak (opis budynku
      w seedzie do poprawy przy migracji w Etapie 4).

### Etap 4 — kalibracja obrony wg Dracopedii — ROZBIEŻNOŚCI
- [x] BattleCalculator ma zaszyte ~połowę wartości źródłowych, a seed (DefenseBonus)
      jeszcze inne — UI pokazuje co innego niż liczy walka. Ujednolicić:
      Smoczy mur 10%, Smocza bariera 10%, Zamek 15% + −10% strat ludności cywilnej.
      → BattleCalculator: Szaniec 5% (+straty cywilów −20%, jak Komando — ta sama
      funkcja, nie kumulują się), Mur 10%, Bariera 10%, Zamek 15% i −10% strat
      cywilów; seed + migracja DracopediaDefenseCalibration (opisy i DefenseBonus
      zsynchronizowane z walką).
- [x] Sieć fortec: sekwencja strat ziemi 6/6/6/4,5/3/1,5% (bez Sieci 10/10/8/6/4/2%)
      zamiast płaskiego bonusu. Sprawdzić u źródła Szaniec i Pospolite ruszenie.
      → CalculateLandCaptured liczy przełamane obrony celu od ostatniego
      przeliczenia (BattleReports); Sieć nie daje już % obrony. Pospolite
      ruszenie wg źródła: broni CAŁA ludność (nie tylko zatrudnieni) + złodzieje,
      współczynnik 2 (Goblin 3, Olbrzym 2,5, Br-Oug 1,5, Gnom 1); usunięty
      niepotwierdzony bonus Enta (+1). Hobbit: ziemia ×0,82.

### Etap 5 — Koszary i Akademia wojskowa jako budynki szkolące — ROZBIEŻNOŚĆ
- [ ] Koszary: 10% hoplitów→E1/turę; Akademia wojskowa: 5% E1→E2/turę
      (Olbrzym 6%, Goblin 4,5%). Zgrać z nauką Trening (TrainingHelper).
- [ ] Konflikt ról: obecna AkademiaWojskowa podwaja szansę przyjścia generała
      (GeneralService) — rozstrzygnąć (przenieść/zostawić jako dodatkową).

### Etap 6 — drobiazgi ekonomiczne — ROZBIEŻNOŚCI
- [ ] Ratusz: Ludzie 20 zł/obywatela (inni 10); podatek bez wojska i złodziei.
- [ ] Kopalnia złota: ~10% szansy/turę na skarb 80–160% produkcji złota z tury
      (zamiast deterministycznych +10%); z Górnictwem odkrywkowym mniejsze,
      ale regularne; Złoty Sierp ×2 szansy.

### Etap 7 — efekty specjalne nauk poziomów 4–5 — BRAK LOGIKI
- [ ] Architektura 4: budynki 3. rzędu bez złota, przyspieszanie budowy −50%.
- [ ] Architektura 5: szybsza budowa rzędów 6–7.
- [ ] Rekrutacja 4–5: zweryfikować sens (karawany nie istnieją) — wdrożyć lub
      świadomie pominąć z adnotacją. Czarodziejstwo 5: 25% (źródło) vs 30% (kod).

Uwaga: kalibracja B10/B11 pozostaje osobno (wymaga żywych graczy).

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
