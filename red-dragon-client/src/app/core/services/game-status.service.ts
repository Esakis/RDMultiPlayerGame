import { Injectable, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Subscription, timer } from 'rxjs';
import { switchMap, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface GameStatus {
  unreadMessages: number;
  reportsSinceReset: number;
  nextResetAt: string | null;
  serverTimeUtc: string;
}

/**
 * Cykliczny status gry dla nagłówka (polling co 60 s): nieprzeczytane wiadomości,
 * raporty z ostatniego przeliczenia, czas następnego przeliczenia.
 */
@Injectable({ providedIn: 'root' })
export class GameStatusService implements OnDestroy {
  private apiUrl = environment.apiUrl;
  readonly status$ = new BehaviorSubject<GameStatus | null>(null);
  private sub?: Subscription;

  constructor(private http: HttpClient) {}

  /** Uruchamia polling (idempotentne — kolejne wywołania nic nie zmieniają). */
  start(): void {
    if (this.sub) return;
    this.sub = timer(0, 60_000).pipe(
      switchMap(() => this.http.get<GameStatus>(`${this.apiUrl}/notification/status`).pipe(
        catchError(() => of(null))
      ))
    ).subscribe(s => { if (s) this.status$.next(s); });
  }

  stop(): void {
    this.sub?.unsubscribe();
    this.sub = undefined;
  }

  /** Natychmiastowe odświeżenie (np. po przeczytaniu wiadomości). */
  refresh(): void {
    this.http.get<GameStatus>(`${this.apiUrl}/notification/status`).subscribe({
      next: s => this.status$.next(s),
      error: () => {}
    });
  }

  ngOnDestroy(): void { this.stop(); }
}
