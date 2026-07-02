import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { AuthService } from './core/services/auth.service';
import { KingdomService } from './core/services/kingdom.service';
import { TurnService } from './core/services/turn.service';
import { LanguageService } from './core/services/language.service';
import { Kingdom } from './core/models/kingdom.model';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  kingdom: Kingdom | null = null;
  showLayout = false;

  constructor(
    private auth: AuthService,
    private kingdomService: KingdomService,
    private turnService: TurnService,
    private language: LanguageService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.language.init();
    this.router.events.pipe(
      filter(e => e instanceof NavigationEnd)
    ).subscribe((e) => {
      const url = (e as NavigationEnd).url;
      // /admin ma własny, pełnoekranowy panel (konto admina nie ma księstwa)
      this.showLayout = !['/login', '/register'].includes(url) && !url.startsWith('/admin');
      if (this.showLayout && this.auth.hasToken()) {
        this.loadKingdom();
      }
    });
  }

  loadKingdom(): void {
    this.kingdomService.getMyKingdom().subscribe({
      next: (k) => this.kingdom = k,
      error: () => {}
    });
  }

  useTurn(): void {
    this.kingdomService.useTurn().subscribe({
      next: (res) => {
        this.loadKingdom();
        // Przejdź na Stolicę i dopiero po zakończeniu nawigacji wyemituj delty,
        // aby świeżo zamontowany dashboard zdążył się zasubskrybować i je pokazać.
        this.router.navigate(['/dashboard']).then(() => {
          this.turnService.emitDeltas(res.deltas || {});
        });
      },
      error: () => {}
    });
  }
}
