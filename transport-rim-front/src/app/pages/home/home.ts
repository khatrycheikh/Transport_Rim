import { Component, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

interface PopularTrip {
  from: string;
  to: string;
  duration: string;
  seats: number;
  price: number;
  imageClass: string;
  landmark: 'monument' | 'mosque' | 'desert' | 'fort';
}

interface Partner {
  name: string;
  subtitle: string;
  textColorClass: string;
  icon?: 'stp' | 'diamond';
}

type BookingType = 'simple' | 'retour' | 'reservations';

@Component({
  selector: 'app-home',
  imports: [FormsModule, RouterLink, DecimalPipe],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {
  protected readonly cities = [
    'Nouakchott',
    'Nouadhibou',
    'Rosso',
    'Kiffa',
    'Ayoun',
    'Néma',
    'Kaédi',
    'Sélibaby',
    'Atar',
    'Tagant',
    'Zouérat',
    'Akjoujt',
  ];

  protected readonly departureCity = signal('');
  protected readonly arrivalCity = signal('');
  protected readonly departureDate = signal('');
  protected readonly passengers = signal(1);

  protected readonly bookingType = signal<BookingType>('simple');

  protected readonly bookingTabs: { value: BookingType; label: string; icon: 'swap' | 'repeat' | 'ticket' }[] = [
    { value: 'simple', label: 'Aller simple', icon: 'swap' },
    { value: 'retour', label: 'Aller-retour', icon: 'repeat' },
    { value: 'reservations', label: 'Mes réservations', icon: 'ticket' },
  ];

  protected readonly heroBadges = [
    { label: 'Sécurisé', icon: 'shield' as const },
    { label: 'Rapide', icon: 'clock' as const },
    { label: 'Support 24/7', icon: 'headset' as const },
  ];

  protected readonly highlights = [
    {
      icon: 'ticket' as const,
      title: 'Réservation facile',
      description: 'Réservez vos billets en quelques clics',
    },
    {
      icon: 'seat' as const,
      title: 'Choix de sièges',
      description: 'Sélectionnez vos sièges préférés',
    },
    {
      icon: 'wallet' as const,
      title: 'Paiement sécurisé',
      description: 'Paiement en ligne 100% sécurisé',
    },
    {
      icon: 'headset' as const,
      title: 'Support client',
      description: 'Notre équipe est disponible 24h/24 et 7j/7',
    },
  ];

  protected readonly popularTrips: PopularTrip[] = [
    { from: 'Nouakchott', to: 'Nouadhibou', duration: '16h 00m', seats: 45, price: 700, imageClass: 'from-sky-400 to-blue-600', landmark: 'monument' },
    { from: 'Nouakchott', to: 'Rosso', duration: '5h 30m', seats: 23, price: 500, imageClass: 'from-amber-400 to-orange-600', landmark: 'mosque' },
    { from: 'Nouakchott', to: 'Kiffa', duration: '8h 15m', seats: 34, price: 1000, imageClass: 'from-stone-400 to-stone-600', landmark: 'desert' },
    { from: 'Nouakchott', to: 'Ayoun', duration: '10h 45m', seats: 28, price: 1200, imageClass: 'from-indigo-400 to-violet-600', landmark: 'fort' },
  ];

  protected readonly partners: Partner[] = [
    { name: 'SONEF', subtitle: 'SOCIÉTÉ NATIONALE DE TRANSPORT', textColorClass: 'text-emerald-600' },
    { name: 'STP', subtitle: 'SOCIÉTÉ DE TRANSPORT PUBLIC', textColorClass: 'text-blue-700', icon: 'stp' },
    { name: 'MAURITANIE VOYAGES', subtitle: '', textColorClass: 'text-slate-900 dark:text-white', icon: 'diamond' },
    { name: 'TVC', subtitle: 'TRANSPORTS ET VOYAGES', textColorClass: 'text-slate-900 dark:text-white' },
    { name: 'STM', subtitle: 'SOCIÉTÉ  TRANSPORT MODERNE', textColorClass: 'text-emerald-600' },
  ];

  constructor(private readonly router: Router) {}

  protected setBookingType(type: BookingType): void {
    this.bookingType.set(type);
  }

  protected search(): void {
    this.router.navigate(['/trajets'], {
      queryParams: {
        depart: this.departureCity() || undefined,
        arrivee: this.arrivalCity() || undefined,
        date: this.departureDate() || undefined,
        passagers: this.passengers(),
      },
    });
  }
}
