import { AfterViewInit, Component, ElementRef, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { LanguageService, AppLang } from '../../../core/services/language.service';
import { GameStatusService, GameStatus } from '../../../core/services/game-status.service';
import { Kingdom } from '../../../core/models/kingdom.model';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit, AfterViewInit, OnDestroy {
  @Input() kingdom: Kingdom | null = null;

  @ViewChild('headerWrap') headerWrap?: ElementRef<HTMLElement>;
  private resizeObserver?: ResizeObserver;

  status: GameStatus | null = null;
  countdown = '';
  private countdownTimer?: ReturnType<typeof setInterval>;

  constructor(private auth: AuthService, public language: LanguageService,
              private router: Router, private gameStatus: GameStatusService) {}

  setLang(lang: AppLang): void { this.language.use(lang); }

  ngOnInit(): void {
    this.gameStatus.start();
    this.gameStatus.status$.subscribe(s => this.status = s);
    this.countdownTimer = setInterval(() => this.updateCountdown(), 1000);
  }

  /** Odliczanie do przeliczenia o 5:00 (na podstawie czasu z serwera). */
  private updateCountdown(): void {
    if (!this.status?.nextResetAt) { this.countdown = ''; return; }
    const ms = new Date(this.status.nextResetAt).getTime() - Date.now();
    if (ms <= 0) { this.countdown = '0:00:00'; this.gameStatus.refresh(); return; }
    const totalSec = Math.floor(ms / 1000);
    const h = Math.floor(totalSec / 3600);
    const m = Math.floor((totalSec % 3600) / 60);
    const s = totalSec % 60;
    this.countdown = `${h}:${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
  }

  ngAfterViewInit(): void {
    const el = this.headerWrap?.nativeElement;
    if (!el) { return; }
    // Header jest position:fixed, więc jego realna wysokość (z zawijaniem paska
    // zasobów na wąskich ekranach) musi być rezerwowana przez treść i panel boczny.
    // Mierzymy ją na bieżąco i wystawiamy jako zmienną CSS --header-h.
    this.updateHeaderHeight();
    if (typeof ResizeObserver !== 'undefined') {
      this.resizeObserver = new ResizeObserver(() => this.updateHeaderHeight());
      this.resizeObserver.observe(el);
    }
  }

  ngOnDestroy(): void {
    this.resizeObserver?.disconnect();
    if (this.countdownTimer) clearInterval(this.countdownTimer);
  }

  private updateHeaderHeight(): void {
    const el = this.headerWrap?.nativeElement;
    if (!el) { return; }
    const height = Math.ceil(el.getBoundingClientRect().height);
    document.documentElement.style.setProperty('--header-h', `${height}px`);
  }

  get username(): string {
    return this.auth.getUser()?.username || '';
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
