import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { AppNotification, NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-notification-panel',
  templateUrl: './notification-panel.component.html',
  styleUrls: ['./notification-panel.component.scss']
})
export class NotificationPanelComponent implements OnInit, OnDestroy {
  items: AppNotification[] = [];
  collapsed = false;

  private sub?: Subscription;
  private lastCount = 0;

  constructor(private notify: NotificationService) {}

  ngOnInit(): void {
    this.sub = this.notify.items$.subscribe(items => {
      // Nowe powiadomienie -> rozwiń panel, żeby było widać na bieżąco.
      if (items.length > this.lastCount) {
        this.collapsed = false;
      }
      this.lastCount = items.length;
      this.items = items;
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  toggle(): void {
    this.collapsed = !this.collapsed;
  }

  dismiss(id: number): void {
    this.notify.dismiss(id);
  }

  clearAll(): void {
    this.notify.clear();
  }
}
