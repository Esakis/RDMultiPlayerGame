import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

export interface RaceInfo {
  name: string;
  books: number;
  stats: number[]; // [łatwość, magia, złodzieje, obrona, ekonomia, atak]
  desc: string;
  img: string;
}

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
  email = '';
  username = '';
  password = '';
  passwordConfirm = '';
  kingdomName = '';
  race = 'Człowiek';
  acceptRules = false;
  error = '';
  loading = false;

  // 10 ras oryginalnego Red Dragon — opisy i charakterystyki (0-100) z oficjalnej
  // strony reddragon.cz/pl: [łatwość, magia, złodzieje, obrona, ekonomia, atak]
  races: RaceInfo[] = [
    { name: 'Człowiek', img: 'assets/img/rasy/czlowiek.png', books: 2, stats: [90, 85, 90, 60, 65, 65],
      desc: 'Wszechstronna rasa o licznej populacji — dobrzy magowie, złodzieje i żołnierze.' },
    { name: 'Elf', img: 'assets/img/rasy/elf.png', books: 3, stats: [60, 90, 75, 70, 80, 60],
      desc: 'Mieszkańcy lasów — silna magia (zwłaszcza biała) i skuteczna obrona.' },
    { name: 'Krasnolud', img: 'assets/img/rasy/krasnolud.png', books: 1, stats: [100, 60, 65, 50, 85, 80],
      desc: 'Twardzi górale — najlepsi budowniczowie, mniejsze straty w walce.' },
    { name: 'Hobbit', img: 'assets/img/rasy/hobbit.png', books: 1, stats: [80, 60, 100, 50, 70, 40],
      desc: 'Najlepsi złodzieje w grze, zaskakująco uparta obrona i dobra farma.' },
    { name: 'Nekromant', img: 'assets/img/rasy/nekromant.png', books: 3, stats: [90, 90, 70, 90, 65, 90],
      desc: 'Hordy nieumarłych nie jedzą i nie biorą żołdu; klęski żywiołowe to ich specjalność.' },
    { name: 'Dżin', img: 'assets/img/rasy/dzin.png', books: 5, stats: [50, 100, 65, 90, 45, 35],
      desc: 'Najlepsi magowie — nikt im nie dorównuje; jako jedyni przechowują manę.' },
    { name: 'Goblin', img: 'assets/img/rasy/goblin.png', books: 0, stats: [80, 65, 80, 50, 50, 95],
      desc: 'Agresorzy z machinami wojennymi; +2 tury dziennie, brak magii.' },
    { name: 'Ent', img: 'assets/img/rasy/ent.png', books: 2, stats: [50, 60, 50, 100, 100, 50],
      desc: 'Najlepsza obrona w grze i znakomita farma; -2 tury dziennie.' },
    { name: 'Olbrzym', img: 'assets/img/rasy/olbrzym.png', books: 1, stats: [70, 55, 55, 70, 60, 100],
      desc: 'Najsilniejszy atak w grze i burzenie budynków; je za dwóch, nie może mieć złodziei.' },
    { name: 'Gnom', img: 'assets/img/rasy/gnom.png', books: 3, stats: [70, 70, 75, 60, 75, 55],
      desc: 'Mistrzowie alchemii z polskiego serwera RD — saperzy wysadzają wrogów, ale machin nie używają wcale.' },
    { name: 'Br-Oug', img: 'assets/img/rasy/broug.png', books: 3, stats: [50, 60, 55, 60, 45, 80],
      desc: 'Płodna rasa z polskiego serwera RD — +4 mieszkańców na akr i najpotężniejsze machiny (8 ataku), ale drogie budowanie.' }
  ];

  statLabels = ['Łatwość', 'Magia', 'Złodzieje', 'Obrona', 'Ekonomia', 'Atak'];

  get selectedRace(): RaceInfo {
    return this.races.find(r => r.name === this.race) ?? this.races[0];
  }

  constructor(private auth: AuthService, private router: Router) {}

  get passwordStrength(): string {
    if (!this.password) return '';
    if (this.password.length < 6) return 'Słabe';
    if (this.password.length < 10) return 'Średnie';
    return 'Silne';
  }

  get passwordStrengthClass(): string {
    const s = this.passwordStrength;
    if (s === 'Słabe') return 'weak';
    if (s === 'Średnie') return 'medium';
    if (s === 'Silne') return 'strong';
    return '';
  }

  get passwordMatch(): string {
    if (!this.passwordConfirm) return '';
    return this.passwordConfirm === this.password ? 'OK' : 'Hasła nie pasują';
  }

  get passwordMatchClass(): string {
    if (!this.passwordConfirm) return '';
    return this.passwordConfirm === this.password ? 'ok' : 'weak';
  }

  onSubmit(): void {
    if (!this.acceptRules) {
      this.error = 'Musisz zaakceptować regulamin!';
      return;
    }
    if (!this.email || !this.username || !this.password || !this.kingdomName) {
      this.error = 'Wypełnij wszystkie pola.';
      return;
    }
    if (this.password.length < 6) {
      this.error = 'Hasło musi mieć minimum 6 znaków!';
      return;
    }
    if (this.password !== this.passwordConfirm) {
      this.error = 'Hasła nie są identyczne!';
      return;
    }
    this.loading = true;
    this.error = '';
    this.auth.register(this.email, this.username, this.password, this.kingdomName, this.race).subscribe({
      next: () => {
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.error = err.error || 'Błąd rejestracji.';
        this.loading = false;
      }
    });
  }
}
