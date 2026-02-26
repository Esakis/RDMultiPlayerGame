import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Kingdom, KingdomSummary, AssignWorkersDto, ServiceResult, TurnResult } from '../models/kingdom.model';

@Injectable({
  providedIn: 'root'
})
export class KingdomService {
  private apiUrl = `${environment.apiUrl}/kingdom`;

  constructor(private http: HttpClient) {}

  getMyKingdom(): Observable<Kingdom> {
    return this.http.get<Kingdom>(`${this.apiUrl}/my-kingdom`);
  }

  getKingdom(id: number): Observable<Kingdom> {
    return this.http.get<Kingdom>(`${this.apiUrl}/${id}`);
  }

  getAllKingdoms(eraId?: number): Observable<KingdomSummary[]> {
    const params = eraId ? `?eraId=${eraId}` : '';
    return this.http.get<KingdomSummary[]>(`${this.apiUrl}/all${params}`);
  }

  useTurn(): Observable<TurnResult> {
    return this.http.post<TurnResult>(`${this.apiUrl}/use-turn`, {});
  }

  assignWorkers(dto: AssignWorkersDto): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/assign-workers`, dto);
  }

  buyLand(amount: number): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/buy-land`, { amount });
  }
}
