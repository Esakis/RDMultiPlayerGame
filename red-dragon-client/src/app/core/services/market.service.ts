import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ServiceResult } from '../models/kingdom.model';

export interface MarketOrder {
  id: number;
  orderType: string;       // 'Sell' | 'Buy'
  resource: string;        // 'Food' | 'Stone' | 'Weapons' | 'Mana'
  quantity: number;
  remainingQuantity: number;
  pricePerUnit: number;
  totalPrice: number;
  kingdomId: number;
  kingdomName: string;
  isOwn: boolean;
  createdAt: string;
}

export interface MarketView {
  hasAccess: boolean;
  noAccessReason?: string;
  orders: MarketOrder[];
  myOrders: MarketOrder[];
}

export interface CreateMarketOrder {
  orderType: string;
  resource: string;
  quantity: number;
  pricePerUnit: number;
}

export interface ExchangeRate {
  resource: string;
  buyPrice: number;   // cena kupna 1 szt. (płacisz złotem)
  sellPrice: number;  // cena skupu 1 szt. (dostajesz złoto)
}

export interface MarketTransaction {
  id: number;
  resource: string;
  quantity: number;
  pricePerUnit: number;
  grossGold: number;
  tax: number;
  iAmBuyer: boolean;
  counterpartyName: string;
  occurredAt: string;
}

@Injectable({ providedIn: 'root' })
export class MarketService {
  private apiUrl = `${environment.apiUrl}/market`;

  constructor(private http: HttpClient) {}

  getMarket(): Observable<MarketView> {
    return this.http.get<MarketView>(this.apiUrl);
  }

  createOrder(order: CreateMarketOrder): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/orders`, order);
  }

  fillOrder(orderId: number, quantity: number): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/fill`, { orderId, quantity });
  }

  cancelOrder(orderId: number): Observable<ServiceResult> {
    return this.http.delete<ServiceResult>(`${this.apiUrl}/orders/${orderId}`);
  }

  getHistory(): Observable<MarketTransaction[]> {
    return this.http.get<MarketTransaction[]>(`${this.apiUrl}/history`);
  }

  getExchangeRates(): Observable<ExchangeRate[]> {
    return this.http.get<ExchangeRate[]>(`${this.apiUrl}/exchange`);
  }

  exchange(resource: string, direction: 'Buy' | 'Sell', quantity: number): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/exchange`, { resource, direction, quantity });
  }
}
