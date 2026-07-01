import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MilitaryUnit, UnitDefinition, RecruitUnitsDto, BattleReport, ServiceResult, TrainingInfo } from '../models/kingdom.model';

export interface PlannedAttack {
  id: number;
  attackerKingdomId: number;
  attackerName: string;
  targetKingdomId: number;
  targetName: string;
  generalId: number;
  generalName: string;
  units: { [unitType: string]: number };
  scheduledFor: string;
  createdAt: string;
}

export interface AttackUnit {
  unitType: string;
  displayName: string;
  quantity: number;
  attackPower: number;
}

export interface AttackOptions {
  kingdomId: number;
  kingdomName: string;
  turnsAvailable: number;
  generals: {
    id: number; name: string; primaryTrait: string; secondaryTrait: string;
    level: number; experience: number; isAvailable: boolean; status: string;
  }[];
  units: AttackUnit[];
}

@Injectable({
  providedIn: 'root'
})
export class MilitaryService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getAvailableUnits(): Observable<UnitDefinition[]> {
    return this.http.get<UnitDefinition[]>(`${this.apiUrl}/military/available-units`);
  }

  getMyArmy(): Observable<MilitaryUnit[]> {
    return this.http.get<MilitaryUnit[]>(`${this.apiUrl}/military/my-army`);
  }

  recruit(dto: RecruitUnitsDto): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/military/recruit`, dto);
  }

  recruitBatch(units: { [key: string]: number }): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/military/recruit-batch`, { units });
  }

  disband(units: { [key: string]: number }): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/military/disband`, { units });
  }

  getTraining(): Observable<TrainingInfo> {
    return this.http.get<TrainingInfo>(`${this.apiUrl}/military/training`);
  }

  setTraining(dto: { trainSoldiers: boolean; trainElite: boolean }): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/military/training`, dto);
  }

  attack(targetKingdomId: number, generalId: number, units: { [key: string]: number },
         attackerKingdomId?: number): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/battle/attack`,
      { targetKingdomId, generalId, units, attackerKingdomId });
  }

  getAttackOptions(kingdomId: number): Observable<AttackOptions> {
    return this.http.get<AttackOptions>(`${this.apiUrl}/battle/attack-options/${kingdomId}`);
  }

  getPlannedAttacks(): Observable<PlannedAttack[]> {
    return this.http.get<PlannedAttack[]>(`${this.apiUrl}/battle/planned`);
  }

  getCoalitionPlannedAttacks(): Observable<PlannedAttack[]> {
    return this.http.get<PlannedAttack[]>(`${this.apiUrl}/battle/planned/coalition`);
  }

  cancelPlannedAttack(id: number): Observable<ServiceResult> {
    return this.http.delete<ServiceResult>(`${this.apiUrl}/battle/planned/${id}`);
  }

  getBattleReports(): Observable<BattleReport[]> {
    return this.http.get<BattleReport[]>(`${this.apiUrl}/battle/reports`);
  }

  getCoalitionBattleReports(): Observable<BattleReport[]> {
    return this.http.get<BattleReport[]>(`${this.apiUrl}/battle/reports/coalition`);
  }
}
