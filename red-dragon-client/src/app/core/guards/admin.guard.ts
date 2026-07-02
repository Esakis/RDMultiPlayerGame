import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AdminGuard implements CanActivate {
  constructor(private auth: AuthService, private router: Router) {}

  canActivate(): boolean {
    if (this.auth.hasToken() && this.auth.isAdmin()) {
      return true;
    }
    this.router.navigate([this.auth.hasToken() ? '/dashboard' : '/login']);
    return false;
  }
}
