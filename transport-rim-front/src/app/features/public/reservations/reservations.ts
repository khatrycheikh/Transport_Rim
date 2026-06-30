import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Reservation } from '../../../core/models/reservation.model';
import { ReservationService } from '../../../core/services/reservation.service';

@Component({
  selector: 'app-reservations',
  imports: [DatePipe, DecimalPipe, RouterLink],
  templateUrl: './reservations.html',
  styleUrl: './reservations.scss',
})
export class Reservations {
  private readonly reservationService = inject(ReservationService);

  protected readonly reservations = signal<Reservation[]>([]);
  protected readonly loading = signal(true);
  protected readonly cancellingId = signal<number | null>(null);

  constructor() {
    this.load();
  }

  private load(): void {
    const ids = this.reservationService.myReservationIds();
    if (ids.length === 0) {
      this.loading.set(false);
      return;
    }

    const requests = ids.map((id) =>
      this.reservationService.getById(id).pipe(catchError(() => of(null))),
    );

    forkJoin(requests).subscribe((results) => {
      this.reservations.set(results.filter((r): r is Reservation => r !== null));
      this.loading.set(false);
    });
  }

  protected cancel(reservation: Reservation): void {
    if (!confirm(`Annuler la réservation pour ${reservation.departureCity} → ${reservation.arrivalCity} ?`)) {
      return;
    }

    this.cancellingId.set(reservation.id);
    this.reservationService.cancel(reservation.id).subscribe({
      next: () => {
        this.reservations.update((list) => list.filter((r) => r.id !== reservation.id));
        this.cancellingId.set(null);
      },
      error: () => this.cancellingId.set(null),
    });
  }
}
