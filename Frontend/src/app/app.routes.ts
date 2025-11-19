import { Routes } from '@angular/router';
import { LoginPageComponent } from './pages/login/login-page.component';
import { DashboardPageComponent } from './pages/dashboard/dashboard-page.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginPageComponent,
  },
  {
    path: 'dashboard',
    component: DashboardPageComponent,
    canActivate: [authGuard]
  },
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'dashboard'
  },
  {
    path:'**',
    redirectTo: 'dashboard'
  }
];
