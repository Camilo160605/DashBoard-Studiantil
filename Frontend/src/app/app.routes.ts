import { Routes } from '@angular/router';
import { LoginPageComponent } from './pages/login/login-page.component';
import { DashboardPageComponent } from './pages/dashboard/dashboard-page.component';

export const routes: Routes = [
    {
        path: 'login',
        component: LoginPageComponent,
    },
    {
        path:'dashboard',
        component: DashboardPageComponent,
    },
    {
        path:'**',
        redirectTo: 'login'
    }


];
