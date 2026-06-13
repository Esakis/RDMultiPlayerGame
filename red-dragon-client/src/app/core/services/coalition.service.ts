import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Coalition, ServiceResult } from '../models/kingdom.model';

export interface ElectionCandidate {
  kingdomId: number;
  name: string;
  votes: number;
  isImperator: boolean;
  isMyVote: boolean;
}

export interface Election {
  hasCoalition: boolean;
  currentImperatorId?: number;
  currentImperatorName?: string;
  myVoteKingdomId?: number;
  totalMembers: number;
  candidates: ElectionCandidate[];
}

export interface War {
  id: number;
  declaringCoalitionId: number;
  declaringName: string;
  targetCoalitionId: number;
  targetName: string;
  declaredAt: string;
  isMyDeclaration: boolean;
  opponentCoalitionId: number;
  opponentName: string;
}

export interface PpsStatus {
  hasCoalition: boolean;
  isBuilding: boolean;
  investedBudulec: number;
  cost: number;
  percent: number;
  coalitionLand: number;
  requiredLand: number;
  landThresholdMet: boolean;
  isLeader: boolean;
  role?: string;
  myBudulecStored: number;
}

@Injectable({
  providedIn: 'root'
})
export class CoalitionService {
  private apiUrl = `${environment.apiUrl}/coalition`;

  constructor(private http: HttpClient) {}

  getPps(): Observable<PpsStatus> {
    return this.http.get<PpsStatus>(`${this.apiUrl}/pps`);
  }

  startPps(): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/pps/start`, {});
  }

  contributePps(budulec: number): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/pps/contribute`, { budulec });
  }

  getElection(): Observable<Election> {
    return this.http.get<Election>(`${this.apiUrl}/election`);
  }

  vote(candidateKingdomId: number): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/vote`, { candidateKingdomId });
  }

  getWars(): Observable<War[]> {
    return this.http.get<War[]>(`${this.apiUrl}/wars`);
  }

  declareWar(targetCoalitionId: number): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/war/declare`, { targetCoalitionId });
  }

  endWar(warId: number): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/war/${warId}/end`, {});
  }

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

  appointMainCommander(kingdomId: number): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/appoint-main-commander`, { kingdomId });
  }

  removeMainCommander(): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/remove-main-commander`, {});
  }
}
