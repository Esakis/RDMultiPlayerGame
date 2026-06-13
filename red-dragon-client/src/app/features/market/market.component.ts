import { Component, OnInit } from '@angular/core';
import { MarketService, MarketOrder, CreateMarketOrder } from '../../core/services/market.service';

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

  constructor(private market: MarketService) {}

  ngOnInit(): void {
    this.load();
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
}
