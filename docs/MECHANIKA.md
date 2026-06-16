# Red Dragon — dokument referencyjny mechaniki oryginalnej gry

Cel: wierne odtworzenie przeglądarkowej gry strategicznej **Red Dragon**
(premiera 1998/2001 w Czechach, polski serwer reddragon.pl od października 2002,
czeski serwer reddragon.cz działał do ~grudnia 2017, potem reaktywowany — rejestracja
obecnie zamknięta, trwają prace nad następcą "Dragescent").

Źródła (kopie w `docs/zrodla/`):
- oficjalny manual gry z `game.reddragon.cz/XaP/html/0/manual/` (Wayback Machine, 2015/2016) — m.in. `vzorce.htm` (wzory!)
- oficjalna strona `reddragon.cz` (CZ/PL) — opisy i charakterystyki 10 ras
- oficjalny blog `reddragon.cz/rdblog` — seria „31. věk" (styczeń 2016) ze szczegółowymi statystykami wszystkich ras
- `urza.cz/private/f.rdx2.zmeny/` — manual generałów, paktów, ras (wariant RDx2)
- polska Wikipedia, Nonsensopedia (terminologia polska), recenzje

Uwaga o wersjach: polski serwer (RD.PL) był mutacją czeskiego RD2. Różnice znane:
przeliczenie o **5:00** (CZ: 4:00), koalicja do **17** księstw (CZ: 16), brak ras Ent
i Wampir w starszych edycjach PL. Implementujemy stan gry wg manuala 2015/2016
i rebalansu „31. wieku" z polskim nazewnictwem.

---

## 1. Czas gry

- **Era (věk)** — pełny cykl gry, kończy się zbudowaniem **Pałacu Sądu Ostatecznego**
  (cz. Palác posledního soudu, „PPS"); zwycięska koalicja trafia do Panteonu / Síň slávy.
- **Przeliczenie (přepočet)** — codziennie o 5:00 rano. Wykonywane są wtedy wszystkie
  zaplanowane akcje (kolejność faz: **złodziejska → magiczna → wojskowa**), potem
  przydział nowych tur. Spada też siła aktywnych zaklęć.
- **Tura (tah)** — podstawowa jednostka akcji gracza. Każdy dzień daje bazowo
  **15 tur** (Goblin/Skřet: 17, Ent: 13). Budynki specjalne **Wieża Czasu** i
  **Pałac Czasu** dają po +1 turze dziennie (Goblin: Wieża Czasu daje +2).
- **Kumulacja tur**: niewykorzystane tury przechodzą na następne dni, maksymalnie
  potrójny dzienny przydział („trojtah") = **49 tur** standardowo
  (Goblin z czasówkami: 58, Ent: 45).
- Start ery: pierwsze dni bez przeliczeń (ochrona/protektorat startowy), 1/111 tur,
  zniżki w protektoracie: budynki infra −50%, budynki specjalne −60%, ziemia −60%.

## 2. Rasy (10) — oficjalne charakterystyki

Format charakterystyki z oficjalnej strony: sześć wartości 0–100
(interpretacja: łatwość gry; magia; złodzieje; obrona; ekonomia; atak).

| Rasa (PL) | CZ | Charakterystyka | Księgi magii | Opis oficjalny (PL) |
|---|---|---|---|---|
| Człowiek | Člověk | 90;85;90;60;65;65 | 2 | wszechstronna rasa, duża populacja |
| Elf | Elf | 60;90;75;70;80;60 | 3 | magiczno-wojskowa, biała magia, las |
| Krasnolud | Trpaslík | 100;60;65;50;85;80 | 1 | twardzi obrońcy, najlepsi budowniczowie |
| Hobbit | Hobit | 80;60;100;50;70;40 | 1 | najlepsi złodzieje, uparta obrona |
| Nekromant | Nekromant | 90;90;70;90;65;90 | 3 | hordy nieumarłych, klęski żywiołowe |
| Dżin | Džin | 50;100;65;90;45;35 | 5 | najlepsi magowie |
| Goblin | Skřet | 80;65;80;50;50;95 | 0 | agresor, machiny wojenne |
| Ent | Ent | 50;60;50;100;100;50 | 2 | najlepsza obrona, świetna farma |
| ~~Wampir~~ | ~~Vampýr~~ | ~~60;85;90;80;40;90~~ | ~~3~~ | **USUNIĘTY z gry** (decyzja 2026-06-16; opis referencyjny) |
| Olbrzym | Obr | 70;55;55;70;60;100 | 0 | najsilniejszy atak, burzenie |

### 2.1 Statystyki szczegółowe (rebalans „31. wieku", oficjalny blog)

Jednostki: **Hoplita** zawsze 1/1 (atak/obrona). **E1** = elita 1. stopnia,
**E2** = elita 2. stopnia. **Machina wojenna** (válečný stroj) — siła/0.

| Rasa | E1 | E2 | Machina | Dom (baza) | Dom max* | Na akr (baza) | Na akr z Wodociągiem |
|---|---|---|---|---|---|---|---|
| Człowiek | 3/3 | 7/7 | 5/0 | 5 | 8 | 3 | 4 (+1) |
| Elf | 4/6 | 8/11 | 5/0 | 3 | 6 | 3 | 3,5 |
| Krasnolud | 5/6 | 10/11 | 5/0 | 3 | 6 | 3 | 3,5 |
| Hobbit | 2/4 | 4/10 | 5/0 | 3 | 6 | 3,5 | 4 |
| Nekromant | 2/1 | 6/3 | 4/0 | 3 | 6 | 3 | 3,5 |
| Dżin | 2/2 | 4/6 | 5/0 | 2 | 6 | 2 | 3,5 (+1,5) |
| Goblin | 2/0 | 6/3 | 5/0 | 3 | 5 | 7 | 7,5 |
| Ent | 2/7 | 5/19 | 5/0 | 2 | 5 | 2 | 2,5 |
| Wampir | 4/2 | 10/5 | 5/0 | 3 | 6 | 3 | 3,5 |
| Olbrzym | 6/6 | 16/10 | 6/0 | 3 | 5 | 2,5 | 3 |

\* Dom max = baza + Wodárna (+0,5; Dżin +1) + Norový systém (+1; Goblin/Olbrzym +0,5)
+ Kanalizacja (+1,5; Dżin +2; Goblin/Olbrzym +1).

### 2.2 Bonusy profesji i cechy rasowe (31. wiek)

**Człowiek**: +10% alchemicy, +20% kupcy, max badań +10%/turę, −5% złodzieje,
8 generałów, generałowie +20% doświadczenia, −10% koszt budynków infra (złoto i infrapunkty),
mechanika **Nauka stosowana** (szkoły: złodziejska/magiczna/wojskowa — bonusy dzienne).

**Elf**: +20% płatnerze, +20% druidzi, +30% magowie, −10% kamieniarze, −10% murarze,
Strašidelný lesík odstrasza 10% armii inwazyjnej, Pałac Magii: wzrost kosztu zaklęć
tylko 9%, biała magia −25% ceny, 1,5× z labiryntu, E1 ma siłę magiczną 0,5 / E2 1,0,
mechanika **Komando łuczników** (+20% obrony sojusznika, −20% własnej).

**Krasnolud**: +20% kamieniarze, +30% płatnerze, +20% murarze, −30% druidzi, −20% magowie,
o 1 mniej limitowane zaklęcie przechodzi, −10% koszt budynków specjalnych, −15% złodzieje,
pakty złodziejskie −10% skuteczności, **straty wojskowe −25%**, zabijają +20% smoków,
mechanika **Dozbrojenie** (do +2 pkt atak/obrona dla E1/E2 za broń: 1. pkt = obszar×50
broni, 2. pkt = obszar×100; reset po przeliczeniu).

**Hobbit**: +30% farmerzy, +10% kupcy, −20% magowie, złodzieje −25% ceny i +25% siły,
Zniszczenie zapasów działa na nich w 50%, obniżki popularności (rewolta, DD, ataki) −50%,
odporny na Zły humor, mniejsze straty ziemi (pierwszy atak 9% zamiast 11%),
mechanika **Hodokvas** (uczta).

**Nekromant**: +20% druidzi, +30% magowie, −25% farmerzy, −50% złodzieje,
armia nie je i nie bierze żołdu, nie umiera w głodzie, odporny na Mor/Kastrację/Płodność,
Mor/Szarańcza/Kastracja/Zły humor −50% ceny, mechanika **Nekromancja**:
armię wyczarowują magowie z ciał (Hřbitov/Cmentarz 100 ciał; zaklęcia Przywołaj
E2/E1/hoplitów/złodziei za cenę 1000; wolni magowie = magowie − 0,5×(armia+złodzieje);
zaklęcie Ofiarowanie: −10% populacji/turę → ciała).

**Dżin**: +40% magowie, +10% druidzi, −30% farmerzy, −10% kamieniarze, −10% murarze,
złodzieje −15%, Pałac Magii: pakty magiczne dżina +5% skuteczności,
Padłe legie ×3 skuteczności, każdy dżin przechowuje 1 manę (mana NIE znika po turze),
zaklęcia Metamagii: Posílená magie (+10% siły zaklęć, +25% ceny) / Zrychlená magie
(−10% ceny, −25% siły), baza 210.

**Goblin**: −20% kamieniarze/murarze/płatnerze, −30% alchemicy/farmerzy/naukowcy,
−50% magowie/druidzi, −20% złodzieje, 0 ksiąg magii, +25% przyrost ludności,
**+2 tury dziennie** (17), Wieża Czasu daje +2 (max 58 tur), max badań −20%,
wieże obronne mieszczą 10 hoplitów (obrona 6) i 10 machin (obrona 100!),
każda jednostka utrzyma 2 machiny, mechanika **Goblińska inżynieria**
(machiny z hoplitami burzą, z E1 +50% siły, z E2 obniżają obronę celu o 20% siły).

**Ent**: +50% farmerzy, +20% naukowcy, −25% atak złodziei, **straty wojskowe −50%**,
**−2 tury dziennie** (13), limitowane zaklęcia przechodzą 3× za przeliczenie,
Ognisty deszcz i Smoczy oddech ×2 straty, przelew E1→E2 wolniejszy (7,5% baza),
sady owocowe mieszczą 100 E2 (każda daje 1/10 jedzenia sadu), mechanika **Gniew Enta**
(po stratach: +100% ataku i +100% burzenia machin na 1 przeliczenie;
próg = obszar×100 pkt; mieszkaniec=10 pkt, budynek=połowa ceny infra).

**Wampir** *(USUNIĘTY z implementacji — 2026-06-16; opis referencyjny oryginału, rasa niedostępna w grze, mechanika Krwawej magii wycięta z kodu)*: +20% alchemicy, +20% magowie, +10% złodzieje, 3 księgi,
25% upitych żołnierzy umiera (akcja Upijanie armii), odporny na Mor,
Stupidita/Somnambul/Ospałość −50% ceny, armia nie je (nie umiera w głodzie),
8 generałów, mechanika **Krwawa magia** (punkty krwi za zabitych wrogów:
atak X/obrona 10/złodzieje 8/magia 2; max 50×obszar; eliksiry 4-poziomowe:
Złodziei +5%/lvl, Ataku +7%/lvl, Skupienia +3%/lvl magów, Krwiożerczości +12,5%/lvl strat).

**Olbrzym**: +30% kamieniarze, +30% murarze, −15% magowie, −15% naukowcy,
−25% atak złodziei, 0 ksiąg, limitowane zaklęcia max 4× za przeliczenie,
**jedzenie 2/mieszkańca**, +1% przelew E1→E2 (baza 11%), +25% burzenie machin,
E1 burzy 0,1 / E2 burzy 0,5 (nie blokują wież), mechanika **Szamanizm**
(totemy: Grabieży / Smokobójstwa / Niszczycielstwa; ładowane zaklęciem
Wezwanie totemu, baza 380; koszt totemu obszar×20, max 1/4 na turę).

## 3. Profesje (zawody)

Dziewięć profesji (+ bezrobotni). Każda pracuje w cechach/budynkach:

| Profesja (PL) | CZ | Produkt |
|---|---|---|
| Farmerzy (Chłopi) | farmáři | jedzenie |
| Kamieniarze | kameníci | kamień |
| Murarze | zedníci | budują budynki (infrapunkty) |
| Kupcy | obchodníci | złoto (konkurują o rynek — limit od obszaru) |
| Alchemicy | alchymisté | złoto (bez konkurencji) |
| Płatnerze | zbrojaři | broń |
| Druidzi | druidi | mana |
| Magowie | mágové | rzucają zaklęcia (siła magiczna) |
| Naukowcy | vědci | punkty nauki (badania + expy generałów) |

**Produktywność profesji** (wszystko w %, z manuala):

```
produktywność = (100 − pn·0,9) · (1+rb/100) · (1+pv/100) · (1+cechy/100)
              · (1+zaklęcie/100) · (1−negat/100) · (1+se/100+oe/100+me/100) · (1+mapa/100)
pn  – procent nowicjuszy (nowicjusz pracuje na 10%)
rb  – bonus rasowy profesji
pv  – procent wynalezienia (badania)
cechy – bonus cechów; zaklęcie – Pracowitość / Magiczne fluidum (magowie, druidzi)
negat – Somnambul / Stupidita
se/oe/me – Świątynia (4; magów 8), Ołtarz (2; magów 4), Monument (2; magów 4) ekonomii/magii
```

**Bonus cechów**: `proc = int(100·(ce/(pr·0,08+ce+99)))` (ce — liczba cechów profesji,
pr — liczba ludzi w profesji).
**Uniwersytety**: `proc = int(100·(un/(pr/3·0,08+un+99)))` (pr — wszyscy w profesjach).
**Kupcy (baza)**: `złoto/kupca = 500·z/(z+ob·10)` (z — obszar z paktami handlowymi,
ob — liczba kupców).
**Szkolenie nowicjuszy** (szkoły/place ćwiczeń):
`p% za turę = 100/(6 − 500·s/(z+100·s+99)) + poziomy profesorów`.

## 4. Populacja i popularność

**Maksimum mieszkańców**:
```
max = int(pd·(do+vo+ns+kn) + uz·(1+(pp/100)·(2+vd+rb)))
pd – liczba domów; do – pojemność bazowa domu (wg rasy, patrz 2.1)
vo – +0,5 Wodárna; ns – +1 Norový systém; kn – +1,5 Kanalizacja (modyfikacje rasowe)
uz – obszar; vd – +0,5 Wodotok (Człowiek +2,5 wg manuala / +1 wg 31. wieku); pp – popularność
rb – bonus rasowy na akr
```
**Przyrost**: `baza = wolne_miejsce·(profesje+bezrobotni+0,75·armia)/3/pojemność`,
min. 10% wolnej pojemności; mnożniki zaklęć: Płodność ×1,3, Szczęście ×1,1,
Pech ×0,9, Kastracja ×0,5, Nevěstinec (burdel) ×1,25.
**Ubytek przy przeludnieniu**: `(1 + nadwyżka·0,333/pojemność)·100%`, max 33%/turę.

**Popularność** (cel: dwukrotność płac; kolejność w turze):
1. +1 za stojący budynek specjalny (każda tura),
2. +1 za Dobry humor, −2 za Zły humor,
3. −1…−15 za niedobór jedzenia,
4. zbliżanie do 2×płace: ±(1 + |2·płace − pop|/10),
5. −15 jeśli brak złota na pensje.

## 5. Zasoby

złoto, jedzenie, kamień, broń, mana (znika po turze — wyjątek: Dżin), infrapunkty
(budulec — produkt murarzy), ziemia (akry), punkty nauki.
Przeliczniki rynkowe (orientacyjne, z mechaniki totemów): broń 300, kamień 100,
mana 100, jedzenie 10 (w złocie).
Smoki żywią się mięsem (mechanika armii koalicyjnej w RDx2).

## 6. Ziemia (pozemky)

**Cena kupna**: `cena = ((z+x)^3,5 − z^3,5) / 600 000` (z — obecny obszar, x — kupowane).
Zdobywanie ziemi atakami: pierwszy atak zabiera ~11% (Hobbit 9%; z Siecią twierdz
wojskowych 9%/7%), kolejne ataki w tym samym przeliczeniu — mniej.
Maks. wzrost księstwa +30%/przeliczenie (+30% z kasy koalicji).

## 7. Budynki

### 7.1 Infrastrukturalne (cena rośnie z obszarem)
```
infrapunkty za budynek = int((149·z/15000 + 1) · (1 − 1,5·budownictwo/100))
złoto za budynek = infrapunkty · 200
ponad 20 000 akrów: int((181 + z/1000) · (1 − 1,5·budownictwo/100))
   Olbrzym: z/2000; Człowiek: ×1,5
```
Znane budynki infra: **Domy**, **Cechy** (po jednym typie na profesję),
**Uniwersytety**, **Wieże obronne** (obrona: `o = x·(1+4·v/(v+400))`, x=15, Człowiek 10;
mieszczą 3 hoplitów; Goblin: 10 hoplitów + 10 machin), **Szkoły**, **Place ćwiczeń**
(szkolenie armii), **Manufaktury**: Sad owocowy (k=400, jedzenie), Kamieniołom (k=40),
Diamentowa kopalnia (k=4000, złoto), Manowe jeziorko (k=40, mana) —
produkcja: `p = (z/(z+m·25))·k·(1+2·inżynieria/100)`, **Smocze legowiska** (Dračí doupě),
**Cmentarz** (tylko Nekromant, 100 ciał).
Limit zapasów infrapunktów: 7500 + obszar/4 (do weryfikacji).

### 7.2 Specjalne (cz. specky; po jednym, dają unikalne efekty)
`cena = pc·(1 + uz/5000)·(1 − 1,5·architektura/100)`; budowane przez murarzy
(infrapunkty idą najpierw na specjalny w budowie); +1 popularności/turę za każdy stojący.

Znane budynki specjalne (z manuala i blogów):
- **Pałac Sądu Ostatecznego (PPS)** — kończy erę, cel gry (buduje koalicja).
- **Pałac Magii (Magický palác)** — wzrost kosztu zaklęć 8% zamiast 10% (Elf 9%, Dżin 6%); odblokowuje pełnię magii.
- **Magiczna soczewka (Magická čočka)** — wcześniejszy stopień łańcucha magicznego.
- **Klasztor smoczych mnichów (Klášter dračích mnichů)** — +1 do obrony jednostek (hoplita broni 2×).
- **Plac ćwiczeń berserkerów (Cvičiště berserkrovství)** — +1 do ataku jednostek.
- **Wieża Czasu (Věž času)** / **Pałac Czasu (Palác času)** — +1 tura dziennie każda (Goblin: WCz +2).
- **Ambasada** — +1 limit paktów (5→6).
- **Pałac (Palác)** — generałowie 6→8.
- **Akademia dowodzenia (Velitelská akademia)** — 2× szansa na przyjście generała.
- **Namiot dowódcy (Velitelský stan)** — +10% siły ataku.
- **Zamek (Hrad)** — +10% obrony.
- **Smoczy mur (Dračí zeď)** — +5% obrony.
- **Smocze wały (Dračí hradby)** — +7% obrony.
- **Świątynia/Ołtarz/Monument** ekonomii, armii i magii — bonusy % (świątynia 4/8,
  ołtarz 2/4, monument 2/4; armia: 8/4/4).
- **Straszny lasek (Strašidelný lesík)** — odstrasza 10% armii inwazyjnej (rasowo Elf).
- **Sieć twierdz wojskowych (Síť vojenských pevností)** — mniejsze straty ziemi.
- **Szpital pod Trzema Krzyżami (Nemocnice u Tří křížů)** — mniejsze straty wojsk.
- **Sztab brygady szybkiego reagowania (Štáb brigády rychlého nasazení)** — obrona.
- **Akademia hoplitów (Hoplítí akademie)** — przelew rekrutów→E1 (18%/turę).
- **Akademia elitarna (Elitní akademie)** — przelew E1→E2 (10%/turę; Ent 7,5%; Olbrzym 11%).
- **Ołtarz wtajemniczenia (Oltář zasvěcení)** — +8% przelewu do E1.
- **Skrzyżowanie szlaków handlowych (Průsečík obchodních cest)** — dostęp do rynku.
- **Zajazd / Karczma (Hospoda)** — obniża oczekiwane płace (popularność).
- **Burdel u Smoczego Ogona (Nevěstinec u dračího ocasu)** — przyrost ludności ×1,25.
- **Wodociągi (Vodárna)**, **Kanalizacja**, **System nor (Norový systém)**,
  **Wodotok** — pojemność domów / zaludnienie akrów (patrz 2.1).
- **Portal**, **Drakodrap** — związane ze smokami (cel Smokobójstwa).
- **Tajemnica materii (Tajemství hmoty)** — rasowy Goblina (wolniejszy spadek zaklęć).
- **Pałac Zmian (Palác změn)** — zmiana rasy w trakcie ery.
- **Imperatorski namiot (Imperátorský stan)** — narzędzia imperatora.
- **Pałac przeglądowy (Přehledový palác)** — rozszerzone przeglądy (wygoda).
- **Paktovač** — zarządzanie paktami koalicji.

## 8. Armia i walka

Jednostki: **hoplici** (1/1; żołd 0,2×płacy), **E1**, **E2** (żołd = płaca·(atak+obrona)/10;
E1 bez żołdu), **machiny wojenne** (siła 5, Człowiek 3, Olbrzym 6; burzą budynki;
z E1 siła +50%; wieże blokują machiny — 3 hoplitów/machina), **smoki**
(bonus siły: ×(1+r/(50+r)) + r·100), **złodzieje** (osobna „armia podziemna"),
**domobrana** (pospolite ruszenie: ludność broni z siłą 2+; Ent/Golem +2, Olbrzym +0,5).

**Siła ataku** (bez generałów):
```
u = { [ (10h−9nh)/10·(1+c) + (10e1−9en1)/10·(s1+c) + (10e2−9en2)/10·(s2+c) ]
      · (1+r/(50+r)) + r·100 + s·x } · (1+0,1·vs) · (1+sa/100+oa/100+ma/100)
h/nh – hoplici/nieprzeszkoleni; e1,e2 – elity; s1,s2 – ich atak bazowy
c – 1 gdy Plac berserkerów; r – smoki; s·x – machiny; vs – Namiot dowódcy
sa/oa/ma – Świątynia/Ołtarz/Monument armii (8/4/4, z ulepszeniem 10/5/5)
```

**Siła obrony** (bez generałów):
```
o = { [ (10h−9nh)/10·(1+k) + (10e1−9en1)/10·(s1+k) + (10e2−9en2)/10·(s2+k)
      + p·(2+k+a)·d + v·(10+nbr)·(1+4v/(v+400)) + khl·(10h−9nh)/(2h) ]
      · (1+r/(50+r)) + r·100 } · (1+0,05·dz) · (1+0,07·dh) · (1+0,1·hr)
      · (1+vo/100) · (1−sl/100) · (1+sa/100+oa/100+ma/100) + le
k – Klasztor mnichów; p – ludzie w profesjach + złodzieje w domu; d – domobrana wł.
a – Ent +1, Olbrzym +0,5; v – wieże (nbr: Człowiek 0, inni 5); khl – limit hoplitów w wieżach (3/wieżę)
dz/dh/hr – Smoczy mur/Smocze wały/Zamek; vo – Tarcza wojenna %; sl – Słabość %
le – Padłe legie: min(siła zaklęcia, liczba wyszkolonych magów; Dżin ×3)
```

**Straty wojskowe**: przy wyrównanych siłach ~15% armii; przewaga atakującego →
straty atakującego ↘ 0%, obrońcy ↗ 30% (i odwrotnie). Krasnolud −25%, Ent −50%.
Cywile: 25% strat armii (bez domobrany), 50% przy udanej obronie z domobraną,
pełne straty przy nieudanej obronie z domobraną.
**Zdobycz ziemi**: pierwszy przechodzący atak ~11% obszaru obrońcy.
**Pakty wojskowe**: broni armia sojusznika pozostawiona w domu (bez wież,
domobrany i legii); skuteczność paktu: 1 pakt 50%, 2 pakty 45%, 3 pakty 40%
(Dżin z Pałacem Magii: +5% dla paktów magicznych).

## 9. Magia

- **Księgi magii** określają dostęp rasy do zaklęć (0–5, patrz tabela ras).
- **Koszt zaklęcia**: `cena = ckp · (1 + ziemia/2000) · (1 − czarodziejstwo/100)`
  (ckp — cena bazowa). Każde kolejne zaklęcie w danym dniu zwiększa koszt o 10%
  (8% z Pałacem Magii; Elf 9%, Dżin 8%/6%).
- **Przywołanie smoka**: dodatkowy mnożnik `(D²·0,0001+0,2)·((max(50,D))/100)²`.
- **Spadek siły zaklęć po przeliczeniu**:
  - biała magia: `nowa = siła·(0,45 + lvlBM/200)·(1+s·0,1) − ziemia/100`
  - czarna magia: `nowa = siła·(0,6 − lvlBM/200) − ziemia/100`
  (lvlBM — najlepszy generał z Białą magią; s=1 dla Goblina z Tajemnicą materii).
- **Limity**: na jeden gubernat przechodzą maks. 2 zaklęcia niszczące („limitowane")
  za przeliczenie (Ent 3, Olbrzym 4 — działają na nich częściej!), to samo zaklęcie 1×.

**Znane zaklęcia** (nazwy CZ → PL):
- Pozytywne (biała magia): Pracovitost (Pracowitość — % produkcji), Magické fluidum
  (Fluid magiczny — % siły magów/druidów), Plodnost (Płodność — przyrost ×1,3),
  Štěstí (Szczęście ×1,1), Dobrá nálada (Dobry humor +1 pop./turę).
- Tarcze: Magické zrcadlo (Zwierciadło magiczne — odbija zaklęcia), Padlé legie
  (Padłe legiony — obrona magów), Vojenský štít (Tarcza wojenna +% obrony),
  Antimagický štít (Tarcza antymagiczna), Magické hradby (Mury magiczne).
- Negatywne (czarna magia): Smůla (Pech ×0,9 przyrostu), Špatná nálada (Zły humor
  −2 pop./turę), Mor (Zaraza — zabija ludność), Kobylky (Szarańcza — zjada jedzenie),
  Somnambul, Ospalost (Ospałość), Stupidita (Głupota — osłabia magów),
  Slabost (Słabość −% obrony), Kastrace (Kastracja — przyrost ×0,5),
  Zničit zásoby (Zniszczenie zapasów).
- Niszczące (limitowane): Dračí dech „DD" (Smoczy oddech — ludność+budynki+popularność),
  Zemětřesení „ZT" (Trzęsienie ziemi — budynki), Ohnivý déšť „OD" (Ognisty deszcz — armia),
  Uragán (Huragan), Zpopelnění zlodějů (Spopielenie złodziei), Povodeň (Powódź).
- Inne: Seslat draka (Przywołaj smoka), Odstranění kouzla (Zdjęcie zaklęcia),
  rasowe: Metamagia (Dżin), Wezwanie totemu (Olbrzym), Przywołania armii i Ofiarowanie
  (Nekromant).

Magowie rzucają (siła rośnie z liczbą magów); DD/ZT na Krasnoluda: 75% szkód
w budynkach, 40% szans na zwalenie budynku specjalnego; DD/OD na Elfa: 50% strat armii.

## 10. Złodzieje

Kupowani za złoto (standard ~1200? — Hobbit 900 wg RDx2 / −25% wg 31. wieku),
nie biorą żołdu. Akcje kosztują tury i złodziei.

**Szansa powodzenia** (atak/obrona — stosunek sił złodziejskich):
- w wojnie: 0,5→0% przejścia (100% wykrycia); 1→50%/50%; 1,5→95%/5%; 2→100%/0% (liniowo),
- poza wojną (wykrycie): 0,5→100%; 1→75%; 1,5→50%; 2→25%; 2,5→0%; zawsze min. 5% wykrycia.
  Sekundarki Maskowania i szpiegostwa modyfikują.
**Straty złodziei**: przy równowadze ~10%; rosną/maleją z przewagą (0–20%).

**Znane akcje złodziejskie**: Sledování gubernátu (Obserwacja — szpiegostwo),
Krádež zásob (Kradzież zapasów), Podněcování revolty (Podżeganie do rewolty — popularność),
bourání infrastruktury (Burzenie budynków), Válka gangů (Wojna gangów — zabija złodziei),
Vyvraždění mágů (Wymordowanie magów), Zabíjení lidí (Zabijanie ludności),
Opíjení armády (Upijanie armii — Wampir: 25% upitych umiera),
Únos generála (Porwanie generała), Vražda generála (Zabójstwo generała).

## 11. Generałowie

Limit 6 (8 z Pałacem; Człowiek i Wampir zawsze 8). Przychodzą losowo
(2× szybciej z Akademią dowodzenia). Poziom z doświadczenia:
`min = ((lvl−1)^4)·100+1, max = (lvl^4)·100` → `lvl ≈ int((exp/100)^0,25)+1`.

**Cechy główne** (zdobywanie expów): Vojevůdce (Wódz — +lvl% siły ataku),
Obránce (Obrońca — +lvl% obrony, liczy się najlepszy), Mág (+lvl/(lvl+50)·100% magii),
Zloděj (Złodziej — jw. dla złodziei), Obchodník (Kupiec — +1,5·lvl/(lvl+50)·100%
produktywności kupców; expy w 14./28./42. turze), Profesor (+lvl% szkolenia/turę; expy 2×obszar).

**Cechy drugorzędne**: Únos generála (Porwanie — 2·lvl% szansy),
Generálovražednictví (Zabójstwo generała — 2·lvl%), Poranění generála (Zranienie — 3 dni),
Magie času (Magia czasu — kradnie 1–4 tur: 2·lvl%, potem połowicznie; +1 tura/2 skradzione;
burzy czasówki lvl/4%), Černá magie (Czarna magia — DD przy ataku 2·lvl%, zdejmuje
białą magię 1,5·lvl%, tarcze lvl%), Drakobijectví (Smokobójstwo — zabija lvl% smoków,
burzy Portal/Drakodrap lvl/4%, smocze legowiska), Sabotáž (Sabotaż — +lvl% zburzonych
budynków, lvl/2% domów, 2·lvl% szansy na budynek specjalny), Krvelačnost a sodomie
(Krwiożerczość — +2·lvl% strat, lvl% w niewolę, burzy lvl% wież), Rabování (Rabunek —
niszczy 2·lvl% zapasów, lvl/2% infrapunktów, połowę zabiera), Léčitelství (Uzdrawianie —
ratuje lvl% poległych; ×2 udany atak, ×4 udana obrona), Bílá magie (Biała magia —
rzuca losowe pozytywne zaklęcia co turę), Maskování a špionáž (Maskowanie i szpiegostwo —
±lvl% wykrycia, wywiad 2·lvl%).

Dozwolone kombinacje: wódz — wszystkie bojowe; obrońca/kupiec/profesor — porwanie,
zabójstwo, zranienie, smokobójstwo, uzdrawianie, biała magia; mág/zloděj — dodatkowo
maskowanie. Generał może zginąć w walce, z rąk złodziei, w labiryncie lub w więzieniu.

## 12. Koalicje (aliancje), pakty, polityka

- Koalicja: maks. **16 księstw** (CZ) / **17** (PL). Prowadzi **Imperator**
  (wybierany demokratycznie), mianuje 1–3 **Wodzów (vojevůdce)**; role: dyplomata, paktownik.
- **Pakty** (między członkami koalicji, limit 5, +1 z Ambasadą; oba gubernaty potwierdzają):
  - handlowy (biały): obszar partnera wlicza się kupcom,
  - magiczny (niebieski): magowie partnera bronią przed magią,
  - wojskowy (zielony): armia partnera (w domu) broni,
  - złodziejski (szary): złodzieje partnera (w domu) bronią.
  Skuteczność: 1 pakt 50%, 2 — 45%, 3 — 40% (na typ).
- **Wojny**: válka (wypowiedziana), přepad (zasadzka — atak bez wypowiedzenia,
  wykonywany o przeliczeniu), multiwojna. Wypowiedzenie wojny: od przeliczenia do 20:00.
- **Kasa koalicji**: ziemia z ataków może iść do kasy (+30% wzrostu z kasy).
- **Cel ery**: wspólna budowa **Pałacu Sądu Ostatecznego**; zwycięzcy w Panteonie.

## 13. Pozostałe systemy

- **Labirynt** — minigra: generałowie eksplorują, zdobywają kości/surowce, mogą zginąć
  (Elf bierze 1,5× surowców).
- **Rynek (trh)** — handel surowcami między graczami; dostęp wymaga Skrzyżowania
  szlaków handlowych; Rychlý trh (szybki rynek) — wygoda płatna.
- **Badania (výzkum)** — punkty nauki od naukowców; dziedziny m.in.:
  čarodějnictví (czarodziejstwo — tańsze zaklęcia), stavebnictví (budownictwo — tańsze
  budynki infra), architektura (tańsze budynki specjalne), inženýrství (inżynieria —
  manufaktury +2%/lvl), výcvik (wyszkolenie — przelewy elit), verbování (werbunek —
  tańsi złodzieje), špionáž (szpiegostwo); maks. przyrost badań: obszar·1,1 pkt/turę
  (Człowiek +10%, Goblin −20%, Dżin +20% nauki).
- **Kredyty** — waluta premium: wygoda (symulatory, szybki rynek, Pałac przeglądowy),
  bez pay-to-win.
- **Mrożenie (zmrazení)** — zawieszenie gubernatu na czas nieobecności.
- **Status nowicjusza** — ochrona przed atakami, osobne statystyki.

## 14. POLSKI SERWER reddragon.pl — manual/2 (2007, pobrany w całości!)

Pełny manual polskiego serwera (40 stron, wersja EN) w `docs/zrodla/manual-pl/`.
Najważniejsze różnice i uzupełnienia względem wersji czeskiej:

### 14.1 Rasy polskiego serwera (10)
Ludzie, **Gnomy** (trytoni.php), Elfy, Hobbici, Krasnoludy (trposi.php), Dżini,
Gobliny (skreti.php), Nekromanci, **Br-Ougowie** (broug.php), Olbrzymy (obri.php).
**Nie było Enta i Wampira** (to rasy czeskie). Jednostki: Rycerz/Paladyn 3/3 i 9/8,
Łucznik/Leśna Zjawa 4/4 i 9/11, Krasnolud Bojowy/Berserker 4/4 i 9/9,
Błotostęp/Nornik 2/4 i 5/9, Bro'Var/Dżin'Beam 2/4 i 6/8, Wilczy Jeździec/Skurut Hai
4/3 i 11/6, Szkielet/Zombi 4/0 i 10/6, Golem/Ogr 5/5 i 14/11 (machiny 7!),
Gnom: Nocny Strażnik/Saper 1/5 i 8/7 (machiny 0!), Br-Oug: Kro-Draag/Ter-Aark
2/2 i 5/6 (machiny 8!).

### 14.2 Zaklęcia — autentyczne ceny bazowe (przy 100 akrach) — magie.php
Podstawowe: Sokole Oko 20, Dobry humor 125, Zdjęcie zaklęcia 125, Pracowitość 340,
Mannamorfoza 85 (mana→złoto 200/szt.). Ks. Mocy: Tarcza antymagiczna 210 (+24%),
Tarcza wojenna 380 (+24%), Szczęście 210 (max 49%), Zwierciadło magiczne 680 (24%),
Padłe legiony 425. Ks. Ziemi: Płodność 210 (+30%), Trzęsienie Ziemi 190, Szarańcza 125,
Zaraza 275 (3%/turę, Olbrzym odporny), Klątwa Padłych Legionów 100. Ks. Ognia:
Zły humor 65, Słabość 85 (−24%), Ognisty Deszcz 340 (2–4%), Pech 65,
Przywołanie Smoka (wzór). Ks. Wiatru: Zniszczenie zapasów 125 (20%!), Huragan 255
(4% profesji), Spopielenie złodziei 210 (5–10%, Goblin odporny), Chochliki 125
(niszczą machiny, Gnom odporny), Smoczy Oddech 1500 (wymaga Pałacu Magicznego).
Ks. Mistyki: Somnambulizm 105 (−50%), Głupota 85 (−25% magów), Fluid magiczny 210
(+49%), Kastracja 85 (−50%).
**Mechanika**: drożenie +10%/zaklęcie (Dżin z Pałacem 9%, Gnom 11%); po turze poziom
drożyzny = (poziom/2)+45, min 100%. Siła zaklęcia 80–120% siły magów (+20% z Soczewką).
Długoterminowe tracą ~50% siły na turę. **Limity**: TZ/SO/OD — 5 na cel (Krasnolud 4,
Goblin 3); Huragan 7 (6/5); Spopielenie 7 (6/0). Księgi: Dżin 5, Elf 4, Nekromant 4,
Człowiek 3, Gnom 3, Br-Oug 3, Krasnolud/Hobbit/Goblin/Olbrzym 1. Pałac Magiczny: +1 księga.

### 14.3 Budynki specjalne — pełne drzewko (sbprehled.php) — WDROŻONE CZĘŚCIOWO
- 500: Soczewka magiczna (+20% zaklęć), Ratusz (podatek 10 zł/mieszkańca),
  Zajazd u Czerwonego Smoka (+1 pop.), Komando (straty cywilów −20%),
  Warsztat many (Mannamorfoza, +20% zaklęć na siebie)
- 5000: Kwatera generała (+20% ataku), Kopalnia złota (10% szans na 80–120% prod.),
  Straszny las (5% armii inwazyjnej ucieka), Warsztat płatnerski (5 broni/poległego;
  wojna: 40–50k broni/przeliczenie), Smoczy Mur (+5%), Sanktuarium Stwórcy (labirynt 2×)
- 20000: Ołtarz Inicjacji (8% hoplitów→E1/turę), Gildia Złodziei (+25%; Hobbit +60%;
  Olbrzym: Gildia Wojowników +1/+2 E2), Szpital (straty obronne −25%),
  Rezydencja generała (+25% exp), Smocza Bariera (+7%), Wieża ciśnień (+0,5 dom; Gnom +1)
- 50000: Piwnice/Kanalizacja (+1 dom; Gnom +2, Elf/Olbrzym +0,5), Burdel Smoczy Ogon
  (przyrost +25%), Zamek (+10% obrony, −10% strat), Mury magiczne (+25% obrony mag.),
  Pospolite ruszenie (cywile siła 2; Goblin 3, Olbrzym 2,5, Br-Oug 1,5; straty 15–25%),
  Rynek (handel), Ambasada (+1 pakt), Medyk (straty w ataku −50%)
- 85000: Akademia wojskowa (5% E1→E2; Olbrzym 6%, Goblin 4,5%), Pałac (+1 atak E2,
  leczy generałów), Port towarowy (400–600k złota/turę; wojna ×2), Koszary
  (10% hoplitów→E1), System fortec obronnych (8% ziemi zamiast 10%),
  Zachodnia Wieża Czasu (+1 tura; Goblin +2), Akwedukt (+0,5/akr; Br-Oug +2,5)
- 110000: Kanały (+1,5 dom; Olbrzym +1, Elf +0,5), Wschodnia Wieża Czasu (+1),
  Smokodrap (przywoływanie smoków; bez niego max 50 smoków), Portal (wabi smoki),
  Pałac Magiczny (Smoczy Oddech + księga; Dżin 9%)
- 200000: Klasztor Smoczych Mnichów (+1 obrona), Obóz Berserkerów (+1 atak),
  **Pałac Sądu Ostatecznego: koalicja >750 000 akrów przez całą budowę, cena bazowa
  10 000 000; atak koalicji resetuje budowę do 0; ukończenie kończy erę, wszyscy
  wracają do 100 akrów; po 100. dniu ery — Czas Apokalipsy (ułatwienie budowy)**

### 14.4 Pozostałe ustalenia PL
- Człowiek: +10% złodzieje/naukowcy, złodziej 1500 zł, +33% nauki, machiny +40% burzenia,
  PTA na zwalenie budynku specjalnego ×2, 3 księgi.
- Elf: druidzi +25% i **mana się nie traci** (rośnie o 1/3 produkcji), murarze −35%
  ale nie potrzebują kamienia, zaklęcia −10% skuteczności, 4 księgi.
- Nekromant: armia i złodzieje nie jedzą; polegli (smoki, zombi, hoplici) odradzają
  się jako Szkielety; zmarły generał wraca jako Generał-Szkielet (nieśmiertelny);
  odzyskuje 100% infrapunktów z rozbiórki; SO/TZ zamieniają 10% zburzonych w nieużytki.
- Goblin: +2 tury, machiny nieniszczalne (zaklęcia/ataki), −20% nauki, do 50% ziemi
  z ataków przekazywane sojusznikom, złodzieje bazowa obrona wojskowa 3.
- Krasnolud: straty wojskowe −50% (także broniąc paktów!), generał 50% szans na
  ranę zamiast śmierci, złodzieje i pakty złodziejskie −15%.
- Olbrzym: bez złodziei; odporny na Zarazę; E1→E2 +1% szybciej; 8 generałów; je 1,5.
- Zdobycz ziemi: 10% (8% z Systemem fortec) — PL.
- Wojna a status: wypowiedzenie do 20:00; zaklęcia tylko na sojuszników
  lub koalicje w stanie wojny; cel nie 4× mniejszy/większy.

## 15. Rozbieżności / do weryfikacji
- Czeski manual 2015/2016 (game.reddragon.cz) — pobrano tylko `vzorce.htm`;
  resztę blokuje rate-limit Wayback (skrypt: /tmp/fetch_manual.sh).
- Bazowe produkcje profesji (jedzenie/kamień/broń na pracownika) — wciąż przybliżone.
- Drzewko badań — `vyzkum` PL pobrany (docs/zrodla/manual-pl/vyzkum.txt) — do wdrożenia.
- Limit zapasów „7500 + obszar/4" — do weryfikacji (Br-Oug: limit ×2).
