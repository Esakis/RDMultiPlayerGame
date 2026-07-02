import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

// Ładowanie tłumaczeń z assets/i18n/{lang}.json
export function httpLoaderFactory(http: HttpClient): TranslateHttpLoader {
  return new TranslateHttpLoader(http, 'assets/i18n/', '.json');
}

import { JwtInterceptor } from './core/interceptors/jwt.interceptor';
import { NumberFormatPipe } from './shared/pipes/number-format.pipe';
import { RaceImagePipe } from './shared/pipes/race-image.pipe';

import { HeaderComponent } from './shared/components/header/header.component';
import { SidebarComponent } from './shared/components/sidebar/sidebar.component';
import { NotificationPanelComponent } from './shared/components/notification-panel/notification-panel.component';
import { NotifyRelayComponent } from './shared/components/notify-relay/notify-relay.component';

import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { BuildingsComponent } from './features/buildings/buildings.component';
import { EconomyComponent } from './features/economy/economy.component';
import { MilitaryComponent } from './features/military/military.component';
import { ResearchComponent } from './features/research/research.component';
import { CoalitionComponent } from './features/coalition/coalition.component';
import { MessagesComponent } from './features/messages/messages.component';
import { RankingComponent } from './features/ranking/ranking.component';
import { ForumComponent } from './features/forum/forum.component';
import { MagicComponent } from './features/magic/magic.component';
import { ThievesComponent } from './features/thieves/thieves.component';
import { GeneralsComponent } from './features/generals/generals.component';
import { MarketComponent } from './features/market/market.component';
import { LabyrinthComponent } from './features/labyrinth/labyrinth.component';
import { DragonsComponent } from './features/dragons/dragons.component';
import { AttackComponent } from './features/attack/attack.component';
import { ReportsComponent } from './features/reports/reports.component';
import { OptionsComponent } from './features/options/options.component';
import { KingdomsComponent } from './features/kingdoms/kingdoms.component';
import { PaymentsComponent } from './features/payments/payments.component';
import { AdminComponent } from './features/admin/admin.component';

@NgModule({
  declarations: [
    AppComponent,
    NumberFormatPipe,
    RaceImagePipe,
    HeaderComponent,
    SidebarComponent,
    NotificationPanelComponent,
    NotifyRelayComponent,
    LoginComponent,
    RegisterComponent,
    DashboardComponent,
    BuildingsComponent,
    EconomyComponent,
    MilitaryComponent,
    ResearchComponent,
    CoalitionComponent,
    MessagesComponent,
    RankingComponent,
    ForumComponent,
    MagicComponent,
    ThievesComponent,
    GeneralsComponent,
    MarketComponent,
    LabyrinthComponent,
    DragonsComponent,
    AttackComponent,
    ReportsComponent,
    OptionsComponent,
    KingdomsComponent,
    PaymentsComponent,
    AdminComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    HttpClientModule,
    CommonModule,
    TranslateModule.forRoot({
      defaultLanguage: 'pl',
      loader: {
        provide: TranslateLoader,
        useFactory: httpLoaderFactory,
        deps: [HttpClient]
      }
    })
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
