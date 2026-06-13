import { Component, OnInit } from '@angular/core';
import { MagicService, SpellListItem } from '../../core/services/magic.service';
import { KingdomService } from '../../core/services/kingdom.service';
import { KingdomSummary } from '../../core/models/kingdom.model';

@Component({
  selector: 'app-magic',
  templateUrl: './magic.component.html',
  styleUrls: ['./magic.component.scss']
})
export class MagicComponent implements OnInit {
  spells: SpellListItem[] = [];
  kingdoms: KingdomSummary[] = [];
  targetId: { [spellType: string]: number } = {};
  message = '';
  error = '';
  loading = true;

  categories = ['Biała', 'Tarcze', 'Czarna', 'Niszcząca', 'Przywołania', 'Pozostałe', 'Rasowe'];

  constructor(private magic: MagicService, private kingdomService: KingdomService) {}

  ngOnInit(): void {
    this.load();
    this.kingdomService.getAllKingdoms().subscribe(k => this.kingdoms = k);
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
    const target = spell.targetType === 'Enemy' ? this.targetId[spell.spellType] : undefined;
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
