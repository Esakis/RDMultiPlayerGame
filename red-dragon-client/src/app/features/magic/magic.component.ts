import { Component, OnInit } from '@angular/core';
import { MagicService, SpellListItem } from '../../core/services/magic.service';
import { KingdomService } from '../../core/services/kingdom.service';
import { CoalitionService, War } from '../../core/services/coalition.service';
import { KingdomSummary, Kingdom } from '../../core/models/kingdom.model';

@Component({
  selector: 'app-magic',
  templateUrl: './magic.component.html',
  styleUrls: ['./magic.component.scss']
})
export class MagicComponent implements OnInit {
  spells: SpellListItem[] = [];
  kingdoms: KingdomSummary[] = [];
  myKingdom: Kingdom | null = null;
  enemyCoalitionIds = new Set<number>();
  targetId: { [spellType: string]: number } = {};
  message = '';
  error = '';
  loading = true;

  categories = ['Biała', 'Tarcze', 'Czarna', 'Niszcząca', 'Przywołania', 'Pozostałe', 'Rasowe'];

  constructor(
    private magic: MagicService,
    private kingdomService: KingdomService,
    private coalitionService: CoalitionService
  ) {}

  ngOnInit(): void {
    this.load();
    this.kingdomService.getAllKingdoms().subscribe(k => this.kingdoms = k);
    this.kingdomService.getMyKingdom().subscribe(k => this.myKingdom = k);
    this.coalitionService.getWars().subscribe({
      next: (wars: War[]) => this.enemyCoalitionIds =
        new Set(wars.filter(w => w.opponentCoalitionId).map(w => w.opponentCoalitionId)),
      error: () => {}
    });
  }

  /** Cele zaklęć ofensywnych: tylko księstwa koalicji w stanie wojny/zasadzki z nami. */
  get enemyTargets(): KingdomSummary[] {
    return this.kingdoms.filter(k =>
      k.id !== this.myKingdom?.id
      && !k.isProtected && !k.isFrozen
      && !!k.coalitionId && this.enemyCoalitionIds.has(k.coalitionId));
  }

  /** Cele białej magii: członkowie własnej koalicji (poza mną). */
  get allyTargets(): KingdomSummary[] {
    const myCoalition = this.myKingdom?.coalitionId;
    if (!myCoalition) return [];
    return this.kingdoms.filter(k =>
      k.id !== this.myKingdom?.id && k.coalitionId === myCoalition && !k.isFrozen);
  }

  /** Czy zaklęcie pozytywne (można je rzucić także na sojusznika). */
  isPositive(s: SpellListItem): boolean {
    return (s.category === 'Biała' || s.category === 'Tarcze') && s.targetType !== 'Enemy';
  }

  setMetamagic(mode: string): void {
    this.kingdomService.setMetamagic(mode).subscribe({
      next: r => { this.message = r.message ?? ''; this.load(); this.kingdomService.getMyKingdom().subscribe(k => this.myKingdom = k); },
      error: e => { this.error = e.error || 'Błąd zmiany metamagii.'; }
    });
  }

  chargeTotem(totem: string): void {
    this.kingdomService.chargeTotem(totem).subscribe({
      next: r => { this.message = r.message ?? ''; this.load(); this.kingdomService.getMyKingdom().subscribe(k => this.myKingdom = k); },
      error: e => { this.error = e.error || 'Błąd ładowania totemu.'; }
    });
  }

  setSchool(school: string): void {
    this.kingdomService.setAppliedScience(school).subscribe({
      next: r => { this.message = r.message ?? ''; this.kingdomService.getMyKingdom().subscribe(k => this.myKingdom = k); },
      error: e => { this.error = e.error || 'Błąd wyboru szkoły.'; }
    });
  }

  load(): void {
    this.magic.getSpells().subscribe({
      next: s => { this.spells = s; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  spellsInCategory(cat: string): SpellListItem[] {
    return this.spells.filter(s => s.category === cat);
  }

  cast(spell: SpellListItem): void {
    this.message = '';
    this.error = '';
    // ofensywne: cel wymagany; biała magia: cel opcjonalny (bez celu = na siebie)
    const target = spell.targetType === 'Enemy' || this.isPositive(spell)
      ? this.targetId[spell.spellType]
      : undefined;
    if (spell.targetType === 'Enemy' && !target) {
      this.error = 'Wybierz cel zaklęcia.';
      return;
    }
    this.magic.cast(spell.spellType, target).subscribe({
      next: r => { this.message = r.message ?? ''; this.load(); },
      error: e => { this.error = e.error || 'Błąd rzucania zaklęcia.'; }
    });
  }
}
