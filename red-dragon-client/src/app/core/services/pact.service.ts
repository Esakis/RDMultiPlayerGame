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
  pactType: string; // 'Handlowy' (domyślny) | 'Magiczny' | 'Wojskowy' | 'Zlodziejski'
}

export interface PactStatus {
  inCoalition: boolean;
  limit: number;      // maks. liczba paktów obronnych (5 + Ambasada)
  usedSlots: number;  // liczba zawartych paktów obronnych
  hasAmbasada: boolean;
  members: PactMember[];
}

@Injectable({ providedIn: 'root' })
export class PactService {
  private apiUrl = `${environment.apiUrl}/pact`;

  constructor(private http: HttpClient) {}

  getStatus(): Observable<PactStatus> {
    return this.http.get<PactStatus>(this.apiUrl);
  }

  setPact(targetKingdomId: number, pactType: string): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/set`, { targetKingdomId, pactType });
  }
}
