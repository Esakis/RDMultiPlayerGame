import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface DragonStatus {
  dragons: number;
  cap: number;
  capSource: string;
  dracoLevel: number;
  dracoBonusPct: number;
  hasPortal: boolean;
  hasSmokodrap: boolean;
  hasMinisterstwo: boolean;
  canSummon: boolean;
  cannotSummonReason?: string;
  powerMultiplier: number;
  flatAttackBonus: number;
  summonCostEstimate: number;
  mana: number;
  expectedArrivalsPerTurn: number;
}

export interface ServiceResult {
  success: boolean;
  message?: string;
}

@Injectable({ providedIn: 'root' })
export class DragonService {
  private apiUrl = `${environment.apiUrl}/dragon`;

  constructor(private http: HttpClient) {}

  getStatus(): Observable<DragonStatus> {
    return this.http.get<DragonStatus>(this.apiUrl);
  }

  summon(): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/summon`, {});
  }
}
