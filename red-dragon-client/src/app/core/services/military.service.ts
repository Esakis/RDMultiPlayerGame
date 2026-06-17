import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MilitaryUnit, UnitDefinition, RecruitUnitsDto, BattleReport, ServiceResult, TrainingInfo } from '../models/kingdom.model';

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

  attack(targetKingdomId: number, units: { [key: string]: number }): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/battle/attack`, { targetKingdomId, units });
  }

  getBattleReports(): Observable<BattleReport[]> {
    return this.http.get<BattleReport[]>(`${this.apiUrl}/battle/reports`);
  }
}
