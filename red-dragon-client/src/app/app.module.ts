import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { CommonModule } from '@angular/common';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

import { JwtInterceptor } from './core/interceptors/jwt.interceptor';
import { NumberFormatPipe } from './shared/pipes/number-format.pipe';

import { HeaderComponent } from './shared/components/header/header.component';
import { SidebarComponent } from './shared/components/sidebar/sidebar.component';

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

@NgModule({
  declarations: [
    AppComponent,
    NumberFormatPipe,
    HeaderComponent,
    SidebarComponent,
    LoginComponent,
    RegisterComponent,
    DashboardComponent,
    BuildingsComponent,
    EconomyComponent,
    MilitaryComponent,
    ResearchComponent,
    CoalitionComponent,
    ReportsComponent,
    MessagesComponent,
    RankingComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    HttpClientModule,
    CommonModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
