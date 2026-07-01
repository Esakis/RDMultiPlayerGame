import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ServiceResult } from '../models/kingdom.model';

export interface General {
  id: number;
  name: string;
  primaryTrait: string;
  secondaryTrait: string;
  experience: number;
  level: number;
  experienceToNextLevel: number;
  status: string;
  isPending: boolean;
  secondaryRerollsLeft: number;
  isAvailable: boolean;
}

@Injectable({ providedIn: 'root' })
export class GeneralService {
  private apiUrl = `${environment.apiUrl}/general`;

  constructor(private http: HttpClient) {}

  getGenerals(): Observable<General[]> {
    return this.http.get<General[]>(this.apiUrl);
  }

  accept(id: number): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/${id}/accept`, {});
  }

  rerollSecondary(id: number): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/${id}/reroll-secondary`, {});
  }

  dismiss(id: number): Observable<ServiceResult> {
    return this.http.delete<ServiceResult>(`${this.apiUrl}/${id}`);
  }
}
