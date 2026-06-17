import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { NotificationService, NotificationType } from '../../../core/services/notification.service';

/**
 * Bezwidokowy mostek: gdy powiązane `message` w komponencie się zmieni,
 * przekazuje je do globalnego panelu powiadomień (zamiast wyświetlać inline).
 * Dzięki temu komunikaty nie spychają treści strony w dół.
 */
@Component({
  selector: 'app-notify-relay',
  template: ''
})
export class NotifyRelayComponent implements OnChanges {
  @Input() message: string | null | undefined;
  @Input() type: NotificationType = 'info';

  constructor(private notify: NotificationService) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['message'] && this.message) {
      this.notify.show(this.message, this.type);
    }
  }
}
