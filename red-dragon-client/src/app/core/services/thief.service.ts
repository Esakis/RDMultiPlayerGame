import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ServiceResult } from '../models/kingdom.model';

export interface ThiefActionItem {
  actionType: string;
  displayName: string;
  description?: string;
  thievesRequired: number;
}

@Injectable({ providedIn: 'root' })
export class ThiefService {
  private apiUrl = `${environment.apiUrl}/thief`;

  constructor(private http: HttpClient) {}

  getActions(): Observable<ThiefActionItem[]> {
    return this.http.get<ThiefActionItem[]>(`${this.apiUrl}/actions`);
  }

  send(actionType: string, targetKingdomId: number, thieves: number): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/send`, { actionType, targetKingdomId, thieves });
  }
}
