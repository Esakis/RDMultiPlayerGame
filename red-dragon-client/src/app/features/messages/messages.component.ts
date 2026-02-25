import { Component, OnInit } from '@angular/core';
import { MessageService } from '../../core/services/message.service';
import { KingdomService } from '../../core/services/kingdom.service';
import { GameMessage, KingdomSummary } from '../../core/models/kingdom.model';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.scss']
})
export class MessagesComponent implements OnInit {
  inbox: GameMessage[] = [];
  sent: GameMessage[] = [];
  kingdoms: KingdomSummary[] = [];
  loading = true;
  message = '';
  tab: 'inbox' | 'sent' | 'compose' = 'inbox';
  newTo = 0;
  newSubject = '';
  newBody = '';

  constructor(private messageService: MessageService, private kingdomService: KingdomService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.messageService.getInbox().subscribe(m => { this.inbox = m; this.loading = false; });
    this.messageService.getSent().subscribe(m => this.sent = m);
    this.kingdomService.getAllKingdoms().subscribe(k => this.kingdoms = k);
  }

  sendMessage(): void {
    if (!this.newTo || !this.newBody) { this.message = 'Wypełnij pola.'; return; }
    this.messageService.send(this.newTo, this.newSubject, this.newBody).subscribe({
      next: (res) => {
        this.message = res.message || 'Wysłano!';
        this.newSubject = ''; this.newBody = ''; this.newTo = 0;
        this.tab = 'sent'; this.load();
        setTimeout(() => this.message = '', 4000);
      },
      error: (err) => { this.message = err.error?.message || 'Błąd'; setTimeout(() => this.message = '', 4000); }
    });
  }

  markRead(id: number): void {
    this.messageService.markAsRead(id).subscribe(() => this.load());
  }
}
