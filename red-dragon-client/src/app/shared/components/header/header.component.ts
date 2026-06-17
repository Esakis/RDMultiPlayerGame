import { AfterViewInit, Component, ElementRef, Input, OnDestroy, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { LanguageService, AppLang } from '../../../core/services/language.service';
import { Kingdom } from '../../../core/models/kingdom.model';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements AfterViewInit, OnDestroy {
  @Input() kingdom: Kingdom | null = null;

  @ViewChild('headerWrap') headerWrap?: ElementRef<HTMLElement>;
  private resizeObserver?: ResizeObserver;

  constructor(private auth: AuthService, public language: LanguageService, private router: Router) {}

  setLang(lang: AppLang): void { this.language.use(lang); }

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
