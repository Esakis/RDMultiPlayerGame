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
}

export interface LabyrinthExpedition {
  generalId: number;
  generalName: string;
  generalLevel: number;
  depth: number;
  pendingGold: number;
  pendingFood: number;
  pendingStone: number;
  pendingWeapons: number;
  pendingMana: number;
  pendingDice: number;
  lastEvent?: string;
}

export interface LabyrinthReward {
  type: string;
  name: string;
  description: string;
  diceCost: number;
  canAfford: boolean;
}

export interface LabyrinthStatus {
  hasActiveExpedition: boolean;
  expedition?: LabyrinthExpedition;
  availableGenerals: LabyrinthGeneral[];
  bankedDice: number;
  turnsAvailable: number;
  dragonLore: number;
  rewards: LabyrinthReward[];
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

  enter(generalId: number): Observable<LabyrinthResult> {
    return this.http.post<LabyrinthResult>(`${this.apiUrl}/enter`, { generalId });
  }

  advance(): Observable<LabyrinthResult> {
    return this.http.post<LabyrinthResult>(`${this.apiUrl}/advance`, {});
  }

  retreat(): Observable<LabyrinthResult> {
    return this.http.post<LabyrinthResult>(`${this.apiUrl}/retreat`, {});
  }

  spend(rewardType: string): Observable<LabyrinthResult> {
    return this.http.post<LabyrinthResult>(`${this.apiUrl}/spend`, { rewardType });
  }
}
