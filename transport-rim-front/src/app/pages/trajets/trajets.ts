import { Component, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Trip } from '../../core/models/trip.model';
import { TripService } from '../../core/services/trip.service';
import { ReservationService } from '../../core/services/reservation.service';
import { AuthService } from '../../core/services/auth.service';
import { extractApiErrorMessage } from '../../core/utils/api-error';

@Component({
  selector: 'app-trajets',
  imports: [DatePipe, DecimalPipe],
  templateUrl: './trajets.html',
  styleUrl: './trajets.scss',
})
export class Trajets {
  private readonly tripService = inject(TripService);
  private readonly reservationService = inject(ReservationService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

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
    this.departureFilter.set((event.target as HTMLInputElement).value);
  }

  protected onArrivalInput(event: Event): void {
    this.arrivalFilter.set((event.target as HTMLInputElement).value);
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

    const input = prompt(`Combien de places voulez-vous réserver pour ${trip.departureCity} → ${trip.arrivalCity} ?`, '1');
    if (!input) return;

    const reservedSeats = Number(input);
    if (!Number.isInteger(reservedSeats) || reservedSeats < 1) return;

    this.reservingTripId.set(trip.id);
    this.reservationService.create({ tripId: trip.id, reservedSeats }).subscribe({
      next: () => {
        this.reservingTripId.set(null);
        this.router.navigateByUrl('/mon-compte/reservations');
      },
      error: (err) => {
        this.reservingTripId.set(null);
        alert(extractApiErrorMessage(err, 'La réservation a échoué.'));
      },
    });
  }
}
