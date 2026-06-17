import { Component, OnInit } from '@angular/core';
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

  resources = [
    { value: 'Food', label: 'Jedzenie' },
    { value: 'Stone', label: 'Kamień' },
    { value: 'Weapons', label: 'Broń' },
    { value: 'Mana', label: 'Mana' }
  ];

  resourceLabels: { [key: string]: string } = {
    Food: 'Jedzenie', Stone: 'Kamień', Weapons: 'Broń', Mana: 'Mana'
  };

  // Formularz nowej oferty
  form: CreateMarketOrder = { orderType: 'Sell', resource: 'Food', quantity: 0, pricePerUnit: 0 };

  // Ilość do zrealizowania per oferta
  fillQty: { [orderId: number]: number } = {};

  // === Pakty sojusznicze ===
  pacts: PactStatus | null = null;
  pactMessage = '';
  pactError = '';

  pactTypes = [
    { value: 'Handlowy', label: 'Handlowy (domyślny)' },
    { value: 'Wojskowy', label: 'Wojskowy' },
    { value: 'Magiczny', label: 'Magiczny' },
    { value: 'Zlodziejski', label: 'Złodziejski' }
  ];

  pactDescriptions: { [key: string]: string } = {
    Handlowy: 'Ziemia sojusznika dolicza się do efektywności Twoich kupców (100%, bez limitu).',
    Wojskowy: 'Armia sojusznika pozostawiona w domu pomaga bronić Twojego księstwa.',
    Magiczny: 'Magowie sojusznika pomagają bronić przed wrogą magią.',
    Zlodziejski: 'Złodzieje sojusznika pomagają bronić przed atakami złodziejskimi.'
  };

  constructor(private market: MarketService, private pactService: PactService) {}

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
      error: () => { this.loading = false; this.error = 'Błąd wczytywania rynku.'; }
    });
    this.market.getHistory().subscribe({ next: h => this.history = h, error: () => this.history = [] });
  }

  resourceLabel(r: string): string {
    return this.resourceLabels[r] ?? r;
  }

  createOrder(): void {
    this.message = '';
    this.error = '';
    if (this.form.quantity <= 0 || this.form.pricePerUnit <= 0) {
      this.error = 'Podaj dodatnią ilość i cenę.';
      return;
    }
    this.market.createOrder(this.form).subscribe({
      next: r => {
        this.message = r.message ?? '';
        this.form.quantity = 0;
        this.form.pricePerUnit = 0;
        this.load();
      },
      error: e => { this.error = e.error || 'Błąd wystawiania oferty.'; }
    });
  }

  fill(order: MarketOrder): void {
    this.message = '';
    this.error = '';
    const qty = this.fillQty[order.id] || order.remainingQuantity;
    this.market.fillOrder(order.id, qty).subscribe({
      next: r => { this.message = r.message ?? ''; this.fillQty[order.id] = 0; this.load(); },
      error: e => { this.error = e.error || 'Błąd realizacji oferty.'; }
    });
  }

  cancel(order: MarketOrder): void {
    if (!confirm('Wycofać ofertę? Depozyt zostanie zwrócony.')) return;
    this.message = '';
    this.error = '';
    this.market.cancelOrder(order.id).subscribe({
      next: r => { this.message = r.message ?? ''; this.load(); },
      error: e => { this.error = e.error || 'Błąd wycofywania oferty.'; }
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
    return this.pactTypes.find(t => t.value === type)?.label ?? type;
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
      error: e => { this.pactError = e.error || 'Błąd zmiany paktu.'; this.loadPacts(); }
    });
  }
}
