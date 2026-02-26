# Red Dragon - Game Design Reference

## RACES (15 total)
### Magical Races (can cast on other kingdoms):
- Ludzie, Elfy, Mroczne Elfy, Dżiny, Gnomy, Br-Ougowie, Meduzy, Ożywieńcy, Plemiona Feniksa

### Non-Magical Races (can only cast on self):
- Krasnoludy, Orkowie, Gobliny, Niziołki, Enty, Olbrzymy

### Special race traits:
- Gobliny: 17 base turns/day (others: 15), max 61 turns (others: 49)
- Ożywieńcy: army doesn't eat, reclaim 100% budulec when demolishing
- Elfy: mana doesn't get sold at turn end, murarze don't need kamienie

## PROFESSIONS (Zawody) - 8 types
### Basic (Podstawowe):
1. **Alchemicy** - produce gold. Workplace: Laboratorium alchemiczne, Guild: Cech słońca
2. **Chłopi** - produce food. Workplace: Gospodarstwo, Guild: Cech słońca
3. **Druidzi** - produce mana. Workplace: Lasy Druidów, Guild: Cech ziemi
4. **Kamieniarze** - produce kamienie (stone). Workplace: Zakłady Kamieniarskie, Guild: Cech ziemi
5. **Murarze** - produce budulec (needs kamienie). Workplace: Warsztaty murarskie, Guild: Cech gwiazd
6. **Płatnerze** - produce broń (weapons). Workplace: Zbrojownie, Guild: Cech gwiazd

### Specialist (Specjalistyczne):
7. **Kupcy** - produce gold (1000 per trained merchant). Workplace: special capacity
8. **Naukowcy** - produce edukacja points (increase other professions efficiency up to +15%)

### Mechanics:
- Each workshop holds 100 workers
- Newly hired workers are "Nowicjusze" (novices) - only 10% efficiency
- Novices train each turn (speed depends on Szkoły buildings + Professor general)
- Wages (pensje): 0-50 gold per worker per turn, affects popularity
- Wages of 50 = 100% popularity, Zajazd u Czerwonego Smoka allows 42 for 100%

## RESOURCES
- **Złoto** (gold) - wages, buildings, trade
- **Jedzenie** (food) - 1 per person + soldier per turn, hunger = mass exodus
- **Kamienie** (stone) - needed by murarze for budulec
- **Budulec** (building points) - for constructing buildings. Limit: 7500 + land/4. Cannot be traded/stolen. Special buildings use current-turn production only (not stored)
- **Broń** (weapons) - for army recruitment
- **Mana** - for spells. Sold at turn start (except Elves)
- **Ziemia** (land/acres) - each building takes 1 acre

## TURNS (Tury)
- Base: 15/day (Gobliny: 17)
- Max accumulation: 49 (Gobliny: 61)
- Wieże czasu buildings: +1 turn each (total +2)
- Zakrzywienie/Załamanie czasu research: +extra turns
- Daily reset at 5:00 AM (Polish time)

### Turn processing order:
1. Sell leftover mana
2. Science points for generals
3. Calculate profession efficiency
4. Gold production (alchemicy + kupcy + manufaktury)
5. Pay wages (pensje)
6. Pay army wages (żołd)
7. Gold from special buildings (ratusz, kopalnia złota, port towarowy)
8. Popularity effects (Zajazd, humor)
9. War machines upkeep/production
10. Food production, consumption, auto-buy if needed, hunger effects
11. Weapons production
12. Stone production, stone purchase for murarze
13. Budulec split: PSO first, then special building, then storage
14. If special building reaches 100% - complete it
15. Mana production
16. Scientist production (edukacja)
17. Popularity from wages
18. Plagues/Chochliki events
19. Population migration
20. Novice training
21. Army training
22. Dragon visits
23. Spell decay
24. Auto-casting, auto-selling

## ECONOMIC BUILDINGS (Budynki gospodarcze)
Cost: 1 acre + gold (scales with land size) + 1 budulec
Types:
- **Domy** (houses) - increase population capacity
- **Warsztaty** (workshops) - hold 100 workers each for a profession
- **Cechy** (guilds) - boost production of a profession
- **Manufaktury** - auto-produce resources without workers
- **Szkoły** (schools) - speed up novice training
- **Konstrukcja machin bojowych** - produce war machines
- **Wieże obronne** (defense towers) - help defend

## SPECIAL BUILDINGS (Budynki specjalne) - TREE structure
Cost scales with land: baseCost * (land/500). Architecture research reduces cost.
Build time = row number in turns (row 1 = 1 turn, row 7 = 7 turns)
Built using current-turn budulec production (not stored budulec).

### Row 1 (cost base 500, 1 turn):
- Zajazd u Czerwonego Smoka (population/popularity)
- Młyn (economy)
- Ratusz (economy)
- Kondensator magiczny (magic)
- Sztab uderzeniowy (military)
- Szaniec (defense)

### Row 2 (cost base 5000, 2 turns):
- Rezydencja Generała (population)
- Kopalnia złota (economy)
- Renowacja broni (economy)
- Tajemnica Odtworzenia (magic)
- Szpital (military)
- Smoczy mur (defense)

### Row 3 (cost base 20000, 3 turns):
- Łaźnia miejska (population)
- Klub odkrywców (economy)
- Świątynia bogactwa Autora (economy)
- Soczewka magiczna (magic)
- Ołtarz Inicjacji (military)
- Smocza bariera (defense)

### Row 4 (cost base 50000, 4 turns):
- System jaskiń (population)
- Skrzyżowanie szlaków handlowych (economy)
- Gildia Złodziei (thieves)
- Ściany magiczne (magic)
- Plac defilad (military)
- Zamek (defense)

### Row 5 (cost base 85000, 5 turns):
- Akwedukt (population)
- Zachodnia wieża czasu (time +1 turn)
- Smokodrap (dragons)
- Lustro magiczne (magic)
- Akademia wojskowa (military)
- Sieć wojennych fortec (defense)

### Row 6 (cost base 110000, 6 turns):
- Kanalizacja (population)
- Wschodnia wieża czasu (time +1 turn)
- Portal (dragons)
- Pałac magiczny (magic)
- Koszary (military)
- Pospolite ruszenie (defense)

### Row 7 (cost base 200000, 7 turns):
- Ministerstwo smoków (dragons)
- Sanktuarium berserkerów (military)
- Klasztor Smoczych Mnichów (defense)

### Prerequisites:
- Must build in order within same column (e.g., Zajazd before Rezydencja before Łaźnia...)
- Row 3+ costs gold when under black magic

## RESEARCH TREE (Nauka)
Scientists produce research points → first goes to Edukacja (max +15%), overflow → discoveries

### 1-level:
- Konstrukcja maszyn drewnianych
- Empiryzm

### 2-level chains:
- Zakrzywienie czasu → Załamanie czasu

### 3-level chains:
- Rekultywacja → Osadnictwo → Górnictwo Odkrywkowe
- Smokoastronomia → Smokoanatomia → Smokodynamika
- Rachunkowość → Buchalteria → Księgowość
- Ostrzenie broni → Naprawa broni → Przekuwanie broni

### 5-level chains:
- Wynalazczość (1-5)
- Architektura (1-5): reduces special building costs, level 4 = no gold cost under black magic + 50% cheaper acceleration
- Inżynieria (1-5): reduces economic building gold cost
- Czarodziejstwo (1-5)
- Trening (1-5)
- Rekrutacja (1-5)

## POPULARITY
- Affected by wages, food availability, special buildings
- 100% = optimal, below = people leave
- Wages of 50 gold = 100% popularity (42 with Zajazd)
- No gold for wages → popularity drops fast

## STARTING VALUES (new kingdom)
- Land: 100 acres
- Gold: 50000
- Food: 10000
- Population: 1000
- Turns: 15/15
