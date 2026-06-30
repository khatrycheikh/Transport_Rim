import { Component, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Trip } from '../../core/models/trip.model';
import { TripService } from '../../core/services/trip.service';
import { AuthService } from '../../core/services/auth.service';

const CITY_IMAGES: Record<string, string> = {
  akjoujt: 'akjoujt.jpg',
  aleg: 'aleg.jpg',
  atar: 'atar.jpg',
  ayoun: 'ayoun.jpg',
  kaedi: 'kaedi.jpg',
  kiffa: 'kiffa.jpg',
  nema: 'nema.jpg',
  nouadhibou: 'nouadhibou.jpg',
  rosso: 'rosso.jpg',
  selibaby: 'selibaby.jpg',
  tidjikja: 'tidjikja.jpg',
  zouerat: 'zouerat.jpg',
};

const FALLBACK_TRIP_IMAGE = '/images/hero-bus.png';

function normalizeCity(city: string): string {
  return city
    .toLowerCase()
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '');
}

@Component({
  selector: 'app-trajets',
  imports: [DatePipe, DecimalPipe],
  templateUrl: './trajets.html',
  styleUrl: './trajets.scss',
})
export class Trajets {
  private readonly tripService = inject(TripService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

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

  protected readonly trips = signal<Trip[]>([]);
  protected readonly loading = signal(true);
  protected readonly reservingTripId = signal<number | null>(null);

  protected readonly departureFilter = signal('');
  protected readonly arrivalFilter = signal('');
  protected readonly dateFilter = signal('');

  constructor(route: ActivatedRoute) {
    const params = route.snapshot.queryParamMap;
    this.departureFilter.set(params.get('depart') ?? '');
    this.arrivalFilter.set(params.get('arrivee') ?? '');
    this.search();
  }

  protected onDepartureInput(event: Event): void {
    this.departureFilter.set((event.target as HTMLSelectElement).value);
  }

  protected onArrivalInput(event: Event): void {
    this.arrivalFilter.set((event.target as HTMLSelectElement).value);
  }

  /** Prefers the arrival city's photo (the destination), falling back to the departure city, then a generic image. */
  protected tripImage(trip: Trip): string {
    const arrival = CITY_IMAGES[normalizeCity(trip.arrivalCity)];
    const departure = CITY_IMAGES[normalizeCity(trip.departureCity)];
    const file = arrival ?? departure;
    return file ? `/images/trips/${file}` : FALLBACK_TRIP_IMAGE;
  }

  protected onDateInput(event: Event): void {
    this.dateFilter.set((event.target as HTMLInputElement).value);
  }

  protected search(): void {
    this.loading.set(true);
    this.tripService
      .search({
        departureCity: this.departureFilter() || undefined,
        arrivalCity: this.arrivalFilter() || undefined,
        date: this.dateFilter() || undefined,
      })
      .subscribe({
        next: (trips) => {
          this.trips.set(trips);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }

  protected reserve(trip: Trip): void {
    if (!this.auth.isAuthenticated()) {
      this.router.navigate(['/connexion'], { queryParams: { returnUrl: '/trajets' } });
      return;
    }

    this.router.navigate(['/trajets', trip.id, 'reserver']);
  }
}
