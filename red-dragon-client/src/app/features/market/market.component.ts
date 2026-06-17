import { Component, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { MarketService, MarketOrder, CreateMarketOrder, MarketTransaction } from '../../core/services/market.service';
import { PactService, PactStatus, PactMember } from '../../core/services/pact.service';

@Component({
  selector: 'app-market',
  templateUrl: './market.component.html',
  styleUrls: ['./market.component.scss']
})
export class MarketComponent implements OnInit {
  hasAccess = false;
  noAccessReason = '';
  orders: MarketOrder[] = [];
  myOrders: MarketOrder[] = [];
  history: MarketTransaction[] = [];
  message = '';
  error = '';
  loading = true;

  private readonly resourceKeys = ['Food', 'Stone', 'Weapons', 'Mana'];

  // Etykiety surowców tłumaczone na bieżąco wg wybranego języka.
  get resources(): { value: string; label: string }[] {
    return this.resourceKeys.map(v => ({ value: v, label: this.resourceLabel(v) }));
  }

  // Formularz nowej oferty
  form: CreateMarketOrder = { orderType: 'Sell', resource: 'Food', quantity: 0, pricePerUnit: 0 };

  // Ilość do zrealizowania per oferta
  fillQty: { [orderId: number]: number } = {};

  // === Pakty sojusznicze ===
  pacts: PactStatus | null = null;
  pactMessage = '';
  pactError = '';

  private readonly pactKeys = ['Handlowy', 'Wojskowy', 'Magiczny', 'Zlodziejski'];

  get pactTypes(): { value: string; label: string }[] {
    return this.pactKeys.map(v => ({ value: v, label: this.pactLabel(v) }));
  }

  constructor(private market: MarketService, private pactService: PactService,
              private translate: TranslateService) {}

  ngOnInit(): void {
    this.load();
    this.loadPacts();
  }

  load(): void {
    this.loading = true;
    this.market.getMarket().subscribe({
      next: v => {
        this.hasAccess = v.hasAccess;
        this.noAccessReason = v.noAccessReason ?? '';
        this.orders = v.orders;
        this.myOrders = v.myOrders;
        this.loading = false;
      },
      error: () => { this.loading = false; this.error = this.translate.instant('mkt.errLoad'); }
    });
    this.market.getHistory().subscribe({ next: h => this.history = h, error: () => this.history = [] });
  }

  resourceLabel(r: string): string {
    const key = `mkt.res.${r}`;
    const t = this.translate.instant(key);
    return t === key ? r : t;
  }

  createOrder(): void {
    this.message = '';
    this.error = '';
    if (this.form.quantity <= 0 || this.form.pricePerUnit <= 0) {
      this.error = this.translate.instant('mkt.errPositive');
      return;
    }
    this.market.createOrder(this.form).subscribe({
      next: r => {
        this.message = r.message ?? '';
        this.form.quantity = 0;
        this.form.pricePerUnit = 0;
        this.load();
      },
      error: e => { this.error = e.error || this.translate.instant('mkt.errCreate'); }
    });
  }

  fill(order: MarketOrder): void {
    this.message = '';
    this.error = '';
    const qty = this.fillQty[order.id] || order.remainingQuantity;
    this.market.fillOrder(order.id, qty).subscribe({
      next: r => { this.message = r.message ?? ''; this.fillQty[order.id] = 0; this.load(); },
      error: e => { this.error = e.error || this.translate.instant('mkt.errFill'); }
    });
  }

  cancel(order: MarketOrder): void {
    if (!confirm(this.translate.instant('mkt.confirmCancel'))) return;
    this.message = '';
    this.error = '';
    this.market.cancelOrder(order.id).subscribe({
      next: r => { this.message = r.message ?? ''; this.load(); },
      error: e => { this.error = e.error || this.translate.instant('mkt.errCancel'); }
    });
  }

  // === Pakty sojusznicze ===
  loadPacts(): void {
    this.pactService.getStatus().subscribe({
      next: p => this.pacts = p,
      error: () => this.pacts = null
    });
  }

  pactLabel(type: string): string {
    const key = `mkt.pact.${type}`;
    const t = this.translate.instant(key);
    return t === key ? type : t;
  }

  pactDescription(type: string): string {
    const key = `mkt.pactDesc.${type}`;
    const t = this.translate.instant(key);
    return t === key ? '' : t;
  }

  pactClass(type: string): string {
    return 'pact-' + type.toLowerCase();
  }

  /** Czy zmiana typu paktu dla danego sojusznika spowodowałaby przekroczenie limitu. */
  pactDisabled(member: PactMember): boolean {
    return !!this.pacts && member.pactType === 'Handlowy' && this.pacts.usedSlots >= this.pacts.limit;
  }

  changePact(member: PactMember, newType: string): void {
    if (newType === member.pactType) return;
    this.pactMessage = '';
    this.pactError = '';
    this.pactService.setPact(member.kingdomId, newType).subscribe({
      next: r => { this.pactMessage = r.message ?? ''; this.loadPacts(); },
      error: e => { this.pactError = e.error || this.translate.instant('mkt.errPact'); this.loadPacts(); }
    });
  }
}
