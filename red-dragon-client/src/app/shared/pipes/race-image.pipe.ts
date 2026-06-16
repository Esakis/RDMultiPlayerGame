import { Pipe, PipeTransform } from '@angular/core';

/** Mapuje nazwę rasy na ścieżkę portretu (assets/img/rasy). */
@Pipe({ name: 'raceImage' })
export class RaceImagePipe implements PipeTransform {
  private readonly map: { [key: string]: string } = {
    'Człowiek': 'czlowiek',
    'Elf': 'elf',
    'Krasnolud': 'krasnolud',
    'Hobbit': 'hobbit',
    'Nekromant': 'nekromant',
    'Dżin': 'dzin',
    'Goblin': 'goblin',
    'Ent': 'ent',
    'Olbrzym': 'olbrzym',
    'Gnom': 'gnom',
    'Br-Oug': 'broug'
  };

  transform(race: string | null | undefined): string {
    const file = race && this.map[race] ? this.map[race] : 'czlowiek';
    return `assets/img/rasy/${file}.png`;
  }
}
