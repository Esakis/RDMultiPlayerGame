import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ServiceResult } from '../models/kingdom.model';

export interface LabyrinthGeneral {
  id: number;
  name: string;
  level: number;
  primaryTrait: string;
  secondaryTrait: string;
}

export interface LabyrinthTreasure {
  type: string;
  name: string;
  description: string;
  riskyForGeneral: boolean;
}

export interface LabyrinthStatus {
  actionPoints: number;
  maxActionPoints: number;
  treasureCost: number;
  generalActionCost: number;
  hasDoubleEntry: boolean;
  turnsUsedThisRecount: number;
  turnsRequiredForTreasure: number;
  canTakeTreasure: boolean;
  fortuneLevel: number;
  availableGenerals: LabyrinthGeneral[];
  treasures: LabyrinthTreasure[];
  lastEvent?: string;
}

export interface LabyrinthResult extends ServiceResult {
  data?: LabyrinthStatus;
}

@Injectable({ providedIn: 'root' })
export class LabyrinthService {
  private apiUrl = `${environment.apiUrl}/labyrinth`;

  constructor(private http: HttpClient) {}

  getStatus(): Observable<LabyrinthStatus> {
    return this.http.get<LabyrinthStatus>(this.apiUrl);
  }

  takeTreasure(generalId: number, treasureType: string): Observable<LabyrinthResult> {
    return this.http.post<LabyrinthResult>(`${this.apiUrl}/treasure`, { generalId, treasureType });
  }

  searchGeneral(generalId: number): Observable<LabyrinthResult> {
    return this.http.post<LabyrinthResult>(`${this.apiUrl}/search-general`, { generalId });
  }

  changeAbility(generalId: number): Observable<LabyrinthResult> {
    return this.http.post<LabyrinthResult>(`${this.apiUrl}/change-ability`, { generalId });
  }
}
