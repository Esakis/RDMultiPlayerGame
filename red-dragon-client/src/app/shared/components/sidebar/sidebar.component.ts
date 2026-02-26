import { Component, EventEmitter, Output } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent {
  @Output() turnUsed = new EventEmitter<void>();

  menuItems = [
    { label: 'Stolica', route: '/dashboard' },
    { label: 'Budowa', route: '/buildings' },
    { label: 'Zatrudnienie', route: '/economy' },
    { label: 'Wojsko', route: '/military' },
    { label: 'Walka', route: '/reports' },
    { label: 'Magia', route: '/research' },
    { label: 'Złodzieje', route: '/ranking' },
    { label: 'Polityka', route: '/coalition' },
    { label: 'Imperator', route: '/ranking' },
    { label: 'Statystyki', route: '/ranking' },
    { label: 'Szkoła', route: '/research' },
    { label: 'Koniec', route: '/messages' },
  ];

  constructor(private auth: AuthService, private router: Router) {}

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }

  onTurnClick(): void {
    this.turnUsed.emit();
  }
}
