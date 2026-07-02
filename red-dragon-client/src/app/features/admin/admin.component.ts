import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AccountService } from '../../core/services/account.service';
import { AuthService } from '../../core/services/auth.service';
import { PaymentRecord } from '../../core/models/kingdom.model';

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.scss']
})
export class AdminComponent implements OnInit {
  price: number | null = null;
  newPrice: number | null = null;
  payments: PaymentRecord[] = [];

  saving = false;
  error = '';
  success = '';

  constructor(
    private account: AccountService,
    private auth: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.account.adminGetPrice().subscribe({
      next: (p) => { this.price = p.price; this.newPrice = p.price; },
      error: (err) => this.error = typeof err?.error === 'string' ? err.error : 'Nie udało się pobrać ustawień.'
    });
    this.loadPayments();
  }

  loadPayments(): void {
    this.account.adminGetPayments().subscribe({
      next: (p) => this.payments = p,
      error: () => {}
    });
  }

  savePrice(): void {
    if (this.newPrice === null || this.newPrice < 0) {
      this.error = 'Opłata nie może być ujemna.';
      return;
    }
    this.saving = true;
    this.error = '';
    this.success = '';
    this.account.adminSetPrice(this.newPrice).subscribe({
      next: (p) => {
        this.saving = false;
        this.price = p.price;
        this.success = `Zapisano — opłata za księstwo wynosi teraz ${p.price} zł.`;
      },
      error: (err) => {
        this.saving = false;
        this.error = typeof err?.error === 'string' ? err.error : 'Nie udało się zapisać.';
      }
    });
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
