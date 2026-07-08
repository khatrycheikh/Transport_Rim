import { Routes } from '@angular/router';
import { Home } from './pages/home/home';
import { Trajets } from './pages/trajets/trajets';
import { Compagnies } from './pages/compagnies/compagnies';
import { About } from './pages/about/about';
import { Contact } from './pages/contact/contact';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { TravelerLayout } from './layouts/traveler-layout/traveler-layout';
import { AdminLayout } from './layouts/admin-layout/admin-layout';
import { CompanyLayout } from './layouts/company-layout/company-layout';
import { Reservations } from './features/public/reservations/reservations';
import { Profil } from './features/public/profil/profil';
import { Booking } from './features/public/booking/booking';
import { Ticket } from './features/public/ticket/ticket';
import { Dashboard } from './features/admin/dashboard/dashboard';
import { Companies } from './features/admin/companies/companies';
import { Users } from './features/admin/users/users';
import { Reservations as AdminReservations } from './features/admin/reservations/reservations';
import { Statistiques } from './features/admin/statistiques/statistiques';
import { Dashboard as CompanyDashboard } from './features/company/dashboard/dashboard';
import { Buses as CompanyBuses } from './features/company/buses/buses';
import { Trajets as CompanyTrajets } from './features/company/trajets/trajets';
import { Reservations as CompanyReservations } from './features/company/reservations/reservations';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'trajets', component: Trajets },
  { path: 'compagnies', component: Compagnies },
  { path: 'a-propos', component: About },
  { path: 'contact', component: Contact },
  { path: 'connexion', component: Login },
  { path: 'inscription', component: Register },
  { path: 'trajets/:id/reserver', component: Booking, canActivate: [authGuard] },
  { path: 'billet/:reservationId', component: Ticket, canActivate: [authGuard] },
  {
    path: 'mon-compte',
    component: TravelerLayout,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'reservations', pathMatch: 'full' },
      { path: 'reservations', component: Reservations },
      { path: 'profil', component: Profil },
    ],
  },
  {
    path: 'admin',
    component: AdminLayout,
    canActivate: [authGuard, roleGuard],
    data: { role: 'Admin' },
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: Dashboard },
      { path: 'users', component: Users },
      { path: 'reservations', component: AdminReservations },
      { path: 'statistiques', component: Statistiques },
      { path: 'compagnies', component: Companies },
    ],
  },
  {
    path: 'compagnie',
    component: CompanyLayout,
    canActivate: [authGuard, roleGuard],
    data: { role: 'Company' },
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: CompanyDashboard },
      { path: 'bus', component: CompanyBuses },
      { path: 'trajets', component: CompanyTrajets },
      { path: 'reservations', component: CompanyReservations },
    ],
  },
  { path: '**', redirectTo: '' },
];
