import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { Reservation } from '../../../core/models/reservation.model';
import { ReservationService } from '../../../core/services/reservation.service';

@Component({
  selector: 'app-admin-reservations',
  imports: [DatePipe, DecimalPipe],
  templateUrl: './reservations.html',
  styleUrl: './reservations.scss',
})
export class Reservations {
  private readonly reservationService = inject(ReservationService);

  protected readonly reservations = signal<Reservation[]>([]);
  protected readonly loading = signal(true);
  protected readonly updatingId = signal<number | null>(null);

  constructor() {
    this.reservationService.getAll().subscribe({
      next: (reservations) => {
        this.reservations.set(reservations);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  protected validate(reservation: Reservation): void {
    this.updatingId.set(reservation.id);
    this.reservationService.setStatus(reservation.id, 'Confirmed').subscribe({
      next: (updated) => {
        this.reservations.update((list) => list.map((r) => (r.id === reservation.id ? updated : r)));
        this.updatingId.set(null);
      },
      error: () => this.updatingId.set(null),
    });
  }

  protected cancel(reservation: Reservation): void {
    if (!confirm(`Annuler la réservation de ${reservation.userName} ?`)) {
      return;
    }

    this.updatingId.set(reservation.id);
    this.reservationService.cancel(reservation.id).subscribe({
      next: () => {
        this.reservations.update((list) => list.filter((r) => r.id !== reservation.id));
        this.updatingId.set(null);
      },
      error: () => this.updatingId.set(null),
    });
  }
}
