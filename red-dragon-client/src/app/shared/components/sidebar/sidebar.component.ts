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
    { label: 'menu.capital', route: '/dashboard' },
    { label: 'menu.buildings', route: '/buildings' },
    { label: 'menu.employment', route: '/economy' },
    { label: 'menu.military', route: '/military' },
    { label: 'menu.combat', route: '/attack' },
    { label: 'menu.reports', route: '/reports' },
    { label: 'menu.magic', route: '/magic' },
    { label: 'menu.thieves', route: '/thieves' },
    { label: 'menu.generals', route: '/generals' },
    { label: 'menu.politics', route: '/coalition' },
    { label: 'menu.market', route: '/market' },
    { label: 'menu.stats', route: '/ranking' },
    { label: 'menu.options', route: '/options' },
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
