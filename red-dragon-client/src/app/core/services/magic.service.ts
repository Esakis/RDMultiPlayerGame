import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ServiceResult } from '../models/kingdom.model';

export interface SpellListItem {
  spellType: string;
  displayName: string;
  category: string;
  description?: string;
  baseCost: number;
  currentCost: number;
  isLimited: boolean;
  targetType: string;
  canCast: boolean;
  cannotCastReason?: string;
}

@Injectable({ providedIn: 'root' })
export class MagicService {
  private apiUrl = `${environment.apiUrl}/magic`;

  constructor(private http: HttpClient) {}

  getSpells(): Observable<SpellListItem[]> {
    return this.http.get<SpellListItem[]>(`${this.apiUrl}/spells`);
  }

  cast(spellType: string, targetKingdomId?: number): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/cast`, { spellType, targetKingdomId });
  }
}
