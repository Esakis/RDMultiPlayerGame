import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AccountKingdom, AdminKingdom, AuthResponse, KingdomLoginInfo, KingdomPrice, PaymentRecord } from '../models/kingdom.model';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private accountUrl = `${environment.apiUrl}/account`;
  private paymentUrl = `${environment.apiUrl}/payment`;
  private adminUrl = `${environment.apiUrl}/admin`;

  constructor(private http: HttpClient) {}

  // Księstwa konta
  getKingdoms(): Observable<AccountKingdom[]> {
    return this.http.get<AccountKingdom[]>(`${this.accountUrl}/kingdoms`);
  }

  createKingdom(name: string, race: string): Observable<AccountKingdom> {
    return this.http.post<AccountKingdom>(`${this.accountUrl}/kingdoms`, { name, race });
  }

  selectKingdom(kingdomId: number): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.accountUrl}/select/${kingdomId}`, {});
  }

  // Płatności
  getPrice(): Observable<KingdomPrice> {
    return this.http.get<KingdomPrice>(`${this.paymentUrl}/price`);
  }

  pay(kingdomId: number, method: string): Observable<PaymentRecord> {
    return this.http.post<PaymentRecord>(`${this.paymentUrl}/pay`, { kingdomId, method });
  }

  getHistory(): Observable<PaymentRecord[]> {
    return this.http.get<PaymentRecord[]>(`${this.paymentUrl}/history`);
  }

  // Panel super admina
  adminGetPrice(): Observable<KingdomPrice> {
    return this.http.get<KingdomPrice>(`${this.adminUrl}/kingdom-price`);
  }

  adminSetPrice(price: number): Observable<KingdomPrice> {
    return this.http.put<KingdomPrice>(`${this.adminUrl}/kingdom-price`, { price });
  }

  adminGetPayments(): Observable<PaymentRecord[]> {
    return this.http.get<PaymentRecord[]>(`${this.adminUrl}/payments`);
  }

  adminGetKingdoms(): Observable<AdminKingdom[]> {
    return this.http.get<AdminKingdom[]>(`${this.adminUrl}/kingdoms`);
  }

  adminLockKingdom(id: number): Observable<void> {
    return this.http.post<void>(`${this.adminUrl}/kingdoms/${id}/lock`, {});
  }

  adminUnlockKingdom(id: number): Observable<void> {
    return this.http.post<void>(`${this.adminUrl}/kingdoms/${id}/unlock`, {});
  }

  adminGetKingdomLogins(id: number): Observable<KingdomLoginInfo[]> {
    return this.http.get<KingdomLoginInfo[]>(`${this.adminUrl}/kingdoms/${id}/logins`);
  }
}
