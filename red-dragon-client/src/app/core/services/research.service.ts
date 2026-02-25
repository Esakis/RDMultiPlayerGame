import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TechDefinition, Research, ServiceResult } from '../models/kingdom.model';

@Injectable({
  providedIn: 'root'
})
export class ResearchService {
  private apiUrl = `${environment.apiUrl}/research`;

  constructor(private http: HttpClient) {}

  getAvailableTechnologies(): Observable<TechDefinition[]> {
    return this.http.get<TechDefinition[]>(`${this.apiUrl}/available`);
  }

  getMyResearch(): Observable<Research[]> {
    return this.http.get<Research[]>(`${this.apiUrl}/my-research`);
  }

  startResearch(techType: string): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/start`, { techType });
  }
}
