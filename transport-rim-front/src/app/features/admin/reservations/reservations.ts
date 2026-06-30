import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { Reservation } from '../../../core/models/reservation.model';
import { ReservationService } from '../../../core/services/reservation.service';
import { PaymentService } from '../../../core/services/payment.service';

@Component({
  selector: 'app-admin-reservations',
  imports: [DatePipe, DecimalPipe],
  templateUrl: './reservations.html',
  styleUrl: './reservations.scss',
})
export class Reservations {
  private readonly reservationService = inject(ReservationService);
  private readonly paymentService = inject(PaymentService);

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

  /** Confirms a payment after manual verification: completes it, which confirms the reservation and generates the ticket. */
  protected validatePayment(reservation: Reservation): void {
    if (!reservation.paymentId) return;

    this.updatingId.set(reservation.id);
    this.paymentService.updateStatus(reservation.paymentId, 'Completed').subscribe({
      next: () => {
        this.reservations.update((list) =>
          list.map((r) =>
            r.id === reservation.id ? { ...r, status: 'Confirmed', paymentStatus: 'Completed' } : r,
          ),
        );
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
