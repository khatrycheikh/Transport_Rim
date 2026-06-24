import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-footer',
  imports: [RouterLink],
  templateUrl: './footer.html',
  styleUrl: './footer.scss',
})
export class Footer {
  protected readonly year = new Date().getFullYear();

  protected readonly usefulLinks = [
    { label: 'Trajets', path: '/trajets' },
    { label: 'Compagnies', path: '/compagnies' },
    { label: 'À propos', path: '/a-propos' },
    { label: 'Contact', path: '/contact' },
  ];

  protected readonly infoLinks = [
    { label: 'Qui sommes-nous ?', path: '/a-propos' },
    { label: 'Conditions générales', path: '/' },
    { label: 'Politique de confidentialité', path: '/' },
    { label: 'FAQ', path: '/' },
  ];
}
