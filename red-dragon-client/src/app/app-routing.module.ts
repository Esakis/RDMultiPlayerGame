import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';

import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { BuildingsComponent } from './features/buildings/buildings.component';
import { EconomyComponent } from './features/economy/economy.component';
import { MilitaryComponent } from './features/military/military.component';
import { ResearchComponent } from './features/research/research.component';
import { CoalitionComponent } from './features/coalition/coalition.component';
import { ReportsComponent } from './features/reports/reports.component';
import { MessagesComponent } from './features/messages/messages.component';
import { RankingComponent } from './features/ranking/ranking.component';

const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard] },
  { path: 'buildings', component: BuildingsComponent, canActivate: [AuthGuard] },
  { path: 'economy', component: EconomyComponent, canActivate: [AuthGuard] },
  { path: 'military', component: MilitaryComponent, canActivate: [AuthGuard] },
  { path: 'research', component: ResearchComponent, canActivate: [AuthGuard] },
  { path: 'coalition', component: CoalitionComponent, canActivate: [AuthGuard] },
  { path: 'reports', component: ReportsComponent, canActivate: [AuthGuard] },
  { path: 'messages', component: MessagesComponent, canActivate: [AuthGuard] },
  { path: 'ranking', component: RankingComponent, canActivate: [AuthGuard] },
  { path: '**', redirectTo: '/dashboard' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
