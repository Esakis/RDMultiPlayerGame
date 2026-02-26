import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ForumPost, CreateForumPost } from '../models/kingdom.model';

@Injectable({
  providedIn: 'root'
})
export class ForumService {
  private apiUrl = `${environment.apiUrl}/forum`;

  constructor(private http: HttpClient) {}

  getGeneralPosts(): Observable<ForumPost[]> {
    return this.http.get<ForumPost[]>(`${this.apiUrl}/general`);
  }

  getCoalitionPosts(): Observable<ForumPost[]> {
    return this.http.get<ForumPost[]>(`${this.apiUrl}/coalition`);
  }

  getPost(id: number): Observable<ForumPost> {
    return this.http.get<ForumPost>(`${this.apiUrl}/${id}`);
  }

  createGeneralPost(dto: CreateForumPost): Observable<ForumPost> {
    return this.http.post<ForumPost>(`${this.apiUrl}/general`, dto);
  }

  createCoalitionPost(dto: CreateForumPost): Observable<ForumPost> {
    return this.http.post<ForumPost>(`${this.apiUrl}/coalition`, dto);
  }
}
