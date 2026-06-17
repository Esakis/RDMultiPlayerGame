import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export type NotificationType = 'info' | 'error';

export interface AppNotification {
  id: number;
  text: string;
  type: NotificationType;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private readonly _items = new BehaviorSubject<AppNotification[]>([]);
  /** Aktywne powiadomienia (najnowsze na początku listy). */
  items$ = this._items.asObservable();

  private counter = 0;

  /**
   * Dodaje ulotne powiadomienie. Znika samo po `ttl` ms (0 = nie znika automatycznie).
   */
  show(text: string, type: NotificationType = 'info', ttl = 5000): void {
    const trimmed = (text || '').trim();
    if (!trimmed) return;

    const item: AppNotification = { id: ++this.counter, text: trimmed, type };
    this._items.next([item, ...this._items.value]);

    if (ttl > 0) {
      setTimeout(() => this.dismiss(item.id), ttl);
    }
  }

  dismiss(id: number): void {
    this._items.next(this._items.value.filter(i => i.id !== id));
  }

  clear(): void {
    this._items.next([]);
  }
}
