import { Component } from '@angular/core';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent {
  menuItems = [
    { label: 'Panel Główny', route: '/dashboard', icon: '🏰' },
    { label: 'Budynki', route: '/buildings', icon: '🏗️' },
    { label: 'Gospodarka', route: '/economy', icon: '💰' },
    { label: 'Armia', route: '/military', icon: '⚔️' },
    { label: 'Badania', route: '/research', icon: '📚' },
    { label: 'Koalicja', route: '/coalition', icon: '🤝' },
    { label: 'Raporty', route: '/reports', icon: '📜' },
    { label: 'Wiadomości', route: '/messages', icon: '✉️' },
    { label: 'Ranking', route: '/ranking', icon: '🏆' },
  ];
}
