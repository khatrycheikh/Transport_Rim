import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-traveler-layout',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './traveler-layout.html',
  styleUrl: './traveler-layout.scss',
})
export class TravelerLayout {
  protected readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly navLinks = [
    { label: 'Rechercher un trajet', path: '/trajets' },
    { label: 'Mes réservations', path: '/mon-compte/reservations' },
    { label: 'Mon profil', path: '/mon-compte/profil' },
  ];

  protected logout(): void {
    this.auth.logout();
    this.router.navigateByUrl('/');
  }
}
