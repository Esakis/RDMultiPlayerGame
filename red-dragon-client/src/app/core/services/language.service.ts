import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

export type AppLang = 'pl' | 'en';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  /** Dostępne języki interfejsu. */
  readonly available: AppLang[] = ['pl', 'en'];

  private static readonly STORAGE_KEY = 'rd_lang';

  constructor(private translate: TranslateService) {}

  /** Inicjalizacja przy starcie aplikacji: zapamiętany wybór lub domyślny polski. */
  init(): void {
    this.translate.addLangs(this.available);
    this.translate.setDefaultLang('pl');
    this.use(this.stored ?? 'pl');
  }

  get current(): AppLang {
    return (this.translate.currentLang as AppLang) || 'pl';
  }

  private get stored(): AppLang | null {
    const v = localStorage.getItem(LanguageService.STORAGE_KEY);
    return v === 'pl' || v === 'en' ? v : null;
  }

  /** Przełącza język i zapamiętuje wybór. */
  use(lang: AppLang): void {
    this.translate.use(lang);
    localStorage.setItem(LanguageService.STORAGE_KEY, lang);
  }
}
