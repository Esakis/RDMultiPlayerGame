import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Coalition, ServiceResult } from '../models/kingdom.model';

@Injectable({
  providedIn: 'root'
})
export class CoalitionService {
  private apiUrl = `${environment.apiUrl}/coalition`;

  constructor(private http: HttpClient) {}

  getCoalitions(eraId?: number): Observable<Coalition[]> {
    const params = eraId ? `?eraId=${eraId}` : '';
    return this.http.get<Coalition[]>(`${this.apiUrl}/list${params}`);
  }

  create(name: string, tag: string): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/create`, { name, tag });
  }

  join(coalitionId: number): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/join`, { coalitionId });
  }

  leave(): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/leave`, {});
  }
}
