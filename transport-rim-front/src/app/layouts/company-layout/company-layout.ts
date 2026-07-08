import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-company-layout',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './company-layout.html',
  styleUrl: './company-layout.scss',
})
export class CompanyLayout {
  protected readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly navLinks = [
    { label: 'Dashboard', path: '/compagnie/dashboard' },
    { label: 'Bus', path: '/compagnie/bus' },
    { label: 'Trajets', path: '/compagnie/trajets' },
    { label: 'Réservations', path: '/compagnie/reservations' },
  ];

  protected logout(): void {
    this.auth.logout();
    this.router.navigateByUrl('/');
  }
}
