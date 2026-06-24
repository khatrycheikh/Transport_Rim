import { Routes } from '@angular/router';
import { Home } from './pages/home/home';
import { Trajets } from './pages/trajets/trajets';
import { Compagnies } from './pages/compagnies/compagnies';
import { About } from './pages/about/about';
import { Contact } from './pages/contact/contact';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'trajets', component: Trajets },
  { path: 'compagnies', component: Compagnies },
  { path: 'a-propos', component: About },
  { path: 'contact', component: Contact },
  { path: 'connexion', component: Login },
  { path: 'inscription', component: Register },
  { path: '**', redirectTo: '' },
];
