import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AccountService } from '../../core/services/account.service';
import { AuthService } from '../../core/services/auth.service';
import { AccountKingdom } from '../../core/models/kingdom.model';

@Component({
  selector: 'app-kingdoms',
  templateUrl: './kingdoms.component.html',
  styleUrls: ['./kingdoms.component.scss']
})
export class KingdomsComponent implements OnInit {
  kingdoms: AccountKingdom[] = [];
  price = 30;
  loading = true;
  error = '';
  info = '';

  showCreateForm = false;
  newName = '';
  newRace = 'Człowiek';
  creating = false;

  races = ['Człowiek', 'Elf', 'Krasnolud', 'Hobbit', 'Nekromant',
           'Dżin', 'Goblin', 'Ent', 'Olbrzym', 'Gnom', 'Br-Oug'];

  constructor(
    private account: AccountService,
    private auth: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.load();
    this.account.getPrice().subscribe({
      next: (p) => this.price = p.price,
      error: () => {}
    });
  }

  load(): void {
    this.loading = true;
    this.account.getKingdoms().subscribe({
      next: (ks) => { this.kingdoms = ks; this.loading = false; },
      error: (err) => { this.error = this.msg(err, 'Nie udało się pobrać księstw.'); this.loading = false; }
    });
  }

  select(k: AccountKingdom): void {
    this.error = '';
    if (k.isSuspended) {
      this.error = 'To księstwo jest zawieszone za brak opłaty — opłać je w panelu płatności.';
      return;
    }
    this.account.selectKingdom(k.id).subscribe({
      next: (res) => {
        this.auth.applyAuth(res);
        this.router.navigate(['/dashboard']);
      },
      error: (err) => this.error = this.msg(err, 'Nie udało się wybrać księstwa.')
    });
  }

  create(): void {
    if (this.newName.trim().length < 3) {
      this.error = 'Nazwa księstwa musi mieć co najmniej 3 znaki.';
      return;
    }
    this.creating = true;
    this.error = '';
    this.account.createKingdom(this.newName.trim(), this.newRace).subscribe({
      next: (k) => {
        this.creating = false;
        this.showCreateForm = false;
        this.newName = '';
        this.info = k.requiresPayment
          ? `Księstwo „${k.name}” założone. Opłać je w ciągu ${k.daysToSuspension} dni (${this.price} zł), inaczej zostanie zawieszone.`
          : `Księstwo „${k.name}” założone — jest darmowe.`;
        this.load();
      },
      error: (err) => { this.creating = false; this.error = this.msg(err, 'Nie udało się założyć księstwa.'); }
    });
  }

  goToPayments(k: AccountKingdom): void {
    this.router.navigate(['/payments'], { queryParams: { kingdomId: k.id } });
  }

  statusClass(k: AccountKingdom): string {
    if (k.isSuspended) return 'suspended';
    if (k.isImperial) return 'imperial';
    if (k.requiresPayment) return 'unpaid';
    return 'ok';
  }

  private msg(err: any, fallback: string): string {
    return typeof err?.error === 'string' ? err.error : fallback;
  }
}
