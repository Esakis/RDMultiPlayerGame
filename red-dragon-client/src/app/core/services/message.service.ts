import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GameMessage, ServiceResult } from '../models/kingdom.model';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  private apiUrl = `${environment.apiUrl}/message`;

  constructor(private http: HttpClient) {}

  getInbox(): Observable<GameMessage[]> {
    return this.http.get<GameMessage[]>(`${this.apiUrl}/inbox`);
  }

  getSent(): Observable<GameMessage[]> {
    return this.http.get<GameMessage[]>(`${this.apiUrl}/sent`);
  }

  send(receiverKingdomId: number, subject: string, body: string): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.apiUrl}/send`, { receiverKingdomId, subject, body });
  }

  markAsRead(id: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/read`, {});
  }
}
