import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-admin-layout',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.scss',
})
export class AdminLayout {
  protected readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly navLinks = [
    { label: 'Dashboard', path: '/admin/dashboard' },
    { label: 'Users', path: '/admin/users' },
    { label: 'Bus', path: '/admin/bus' },
    { label: 'Trajets', path: '/admin/trajets' },
    { label: 'Réservations', path: '/admin/reservations' },
    { label: 'Statistiques', path: '/admin/statistiques' },
    { label: 'Compagnies', path: '/admin/compagnies' },
  ];

  protected logout(): void {
    this.auth.logout();
    this.router.navigateByUrl('/');
  }
}
