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
  kingdomName = '';
  error = '';
  loading = false;

  constructor(private auth: AuthService, private router: Router) {}

  onSubmit(): void {
    if (!this.email || !this.username || !this.password || !this.kingdomName) {
      this.error = 'Wypełnij wszystkie pola.';
      return;
    }
    if (this.password.length < 6) {
      this.error = 'Hasło musi mieć co najmniej 6 znaków.';
      return;
    }
    this.loading = true;
    this.error = '';
    this.auth.register(this.email, this.username, this.password, this.kingdomName).subscribe({
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
