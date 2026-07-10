import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ServiceResult } from '../models/kingdom.model';

export interface PactMember {
  kingdomId: number;
  name: string;
  race: string;
  land: number;
  // Czy członek uczestniczy w wymianie handlowej koalicji.
  tradePactEnabled: boolean;
  // Aktywne typy paktów OBRONNYCH z tym księstwem (Magiczny, Wojskowy, Zlodziejski).
  activePacts: string[];
  // Typy zawarte po ostatnim przeliczeniu — do przeliczenia działają z połową wartości.
  halfPacts: string[];
}

export interface PactStatus {
  inCoalition: boolean;
  limit: number;      // maks. łączna liczba paktów obronnych (5 + Ambasada)
  usedSlots: number;  // liczba zawartych paktów obronnych
  hasAmbasada: boolean;
  // Pakt handlowy (kupiecki) — bez partnera, udział w wymianie koalicji.
  tradePactEnabled: boolean;
  tradePactHalf: boolean;
  members: PactMember[];
}

@Injectable({ providedIn: 'root' })
export class PactService {
  private apiUrl = `${environment.apiUrl}/pact`;

  constructor(private http: HttpClient) {}

  getStatus(): Observable<PactStatus> {
    return this.http.get<PactStatus>(this.apiUrl);
  }

  setPact(targetKingdomId: number, pactType: string, active: boolean): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/set`, { targetKingdomId, pactType, active });
  }

  setTradePact(enabled: boolean): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/trade`, { enabled });
  }
}
