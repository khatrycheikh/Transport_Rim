import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ThemeService } from '../../core/theme.service';

@Component({
  selector: 'app-header',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './header.html',
  styleUrl: './header.scss',
})
export class Header {
  protected readonly theme = inject(ThemeService);

  protected readonly navLinks = [
    { label: 'Accueil', path: '/' },
    { label: 'Trajets', path: '/trajets' },
    { label: 'Compagnies', path: '/compagnies' },
    { label: 'À propos', path: '/a-propos' },
    { label: 'Contact', path: '/contact' },
  ];
}
