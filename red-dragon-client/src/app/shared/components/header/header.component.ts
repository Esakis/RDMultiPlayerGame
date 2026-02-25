import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { Kingdom } from '../../../core/models/kingdom.model';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent {
  @Input() kingdom: Kingdom | null = null;

  constructor(private auth: AuthService, private router: Router) {}

  get username(): string {
    return this.auth.getUser()?.username || '';
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
