import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AccountService } from '../../core/services/account.service';
import { AccountKingdom, PaymentRecord } from '../../core/models/kingdom.model';

@Component({
  selector: 'app-payments',
  templateUrl: './payments.component.html',
  styleUrls: ['./payments.component.scss']
})
export class PaymentsComponent implements OnInit {
  unpaidKingdoms: AccountKingdom[] = [];
  history: PaymentRecord[] = [];
  price = 30;

  selectedKingdomId: number | null = null;
  method = 'BLIK';
  methods = ['BLIK', 'Karta', 'Przelew'];

  paying = false;
  error = '';
  success = '';

  constructor(
    private account: AccountService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    const preselect = Number(this.route.snapshot.queryParamMap.get('kingdomId'));
    if (preselect) this.selectedKingdomId = preselect;

    this.account.getPrice().subscribe({ next: (p) => this.price = p.price, error: () => {} });
    this.load();
  }

  load(): void {
    this.account.getKingdoms().subscribe({
      next: (ks) => {
        this.unpaidKingdoms = ks.filter(k => k.requiresPayment);
        if (this.selectedKingdomId && !this.unpaidKingdoms.some(k => k.id === this.selectedKingdomId)) {
          this.selectedKingdomId = null;
        }
        if (!this.selectedKingdomId && this.unpaidKingdoms.length === 1) {
          this.selectedKingdomId = this.unpaidKingdoms[0].id;
        }
      },
      error: () => {}
    });
    this.account.getHistory().subscribe({
      next: (h) => this.history = h,
      error: () => {}
    });
  }

  pay(): void {
    if (!this.selectedKingdomId) {
      this.error = 'Wybierz księstwo do opłacenia.';
      return;
    }
    this.paying = true;
    this.error = '';
    this.success = '';
    this.account.pay(this.selectedKingdomId, this.method).subscribe({
      next: (p) => {
        this.paying = false;
        this.success = `Opłacono księstwo „${p.kingdomName}” (${p.amount} zł, ${p.method}). Księstwo jest odblokowane.`;
        this.selectedKingdomId = null;
        this.load();
      },
      error: (err) => {
        this.paying = false;
        this.error = typeof err?.error === 'string' ? err.error : 'Płatność nie powiodła się.';
      }
    });
  }

  goToKingdoms(): void {
    this.router.navigate(['/kingdoms']);
  }
}
