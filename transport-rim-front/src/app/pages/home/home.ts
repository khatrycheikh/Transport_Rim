import { Component, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MAURITANIA_CITIES } from '../../core/constants/cities';

interface PopularTrip {
  from: string;
  to: string;
  duration: string;
  seats: number;
  price: number;
  image: string;
}

interface Partner {
  name: string;
  subtitle: string;
  textColorClass: string;
  icon?: 'diamond';
}

type BookingType = 'simple' | 'retour' | 'reservations';

@Component({
  selector: 'app-home',
  imports: [FormsModule, RouterLink, DecimalPipe],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {
  protected readonly cities = MAURITANIA_CITIES;

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
      bgClass: 'bg-orange-100 dark:bg-orange-500/15',
      textClass: 'text-orange-600',
    },
    {
      icon: 'seat' as const,
      title: 'Choix de sièges',
      description: 'Sélectionnez vos sièges préférés',
      bgClass: 'bg-emerald-100 dark:bg-emerald-500/15',
      textClass: 'text-emerald-600',
    },
    {
      icon: 'wallet' as const,
      title: 'Paiement sécurisé',
      description: 'Paiement en ligne 100% sécurisé',
      bgClass: 'bg-blue-100 dark:bg-blue-500/15',
      textClass: 'text-blue-600',
    },
    {
      icon: 'headset' as const,
      title: 'Support client',
      description: 'Notre équipe est disponible 24h/24 et 7j/7',
      bgClass: 'bg-violet-100 dark:bg-violet-500/15',
      textClass: 'text-violet-600',
    },
  ];

  protected readonly popularTrips: PopularTrip[] = [
    { from: 'Nouakchott', to: 'Nouadhibou', duration: '16h 00m', seats: 45, price: 700, image: '/images/trips/nouadhibou.jpg' },
    { from: 'Nouakchott', to: 'Rosso', duration: '5h 30m', seats: 23, price: 500, image: '/images/trips/rosso.jpg' },
    { from: 'Nouakchott', to: 'Atar', duration: '8h 15m', seats: 34, price: 1000, image: '/images/trips/atar.jpg' },
    { from: 'Nouakchott', to: 'Ayoun', duration: '10h 45m', seats: 28, price: 1200, image: '/images/trips/ayoun.jpg' },
  ];

  protected readonly partners: Partner[] = [
    { name: 'SONEF', subtitle: 'SOCIÉTÉ NATIONALE DE TRANSPORT', textColorClass: 'text-emerald-600' },
    { name: 'Nour', subtitle: 'TRANSPORT NOUR', textColorClass: 'text-blue-700' },
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
