import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

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
  race = 'Ludzie';
  acceptRules = false;
  error = '';
  loading = false;

  races = [
    'Ludzie', 'Krasnoludy', 'Elfy', 'Hobbity', 'Gnomy',
    'Orkowie', 'Gobliny', 'Trolle', 'Ogry', 'Barbarzyńcy',
    'Mroczne Elfy', 'Nieumarli', 'Jaszczuroludzie', 'Demony', 'Smoki'
  ];

  magicRaces = [
    'Elfy', 'Gnomy', 'Mroczne Elfy', 'Nieumarli', 'Demony', 'Smoki'
  ];

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
