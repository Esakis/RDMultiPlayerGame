import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Building, BuildingDefinition, ConstructBuildingDto, ServiceResult } from '../models/kingdom.model';

@Injectable({
  providedIn: 'root'
})
export class BuildingService {
  private apiUrl = `${environment.apiUrl}/building`;

  constructor(private http: HttpClient) {}

  getAvailableBuildings(): Observable<BuildingDefinition[]> {
    return this.http.get<BuildingDefinition[]>(`${this.apiUrl}/available`);
  }

  getMyBuildings(): Observable<Building[]> {
    return this.http.get<Building[]>(`${this.apiUrl}/my-buildings`);
  }

  construct(dto: ConstructBuildingDto): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/construct`, dto);
  }
}
