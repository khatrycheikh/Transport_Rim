import { DecimalPipe, DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { extractApiErrorMessage } from '../../../core/utils/api-error';
import { PaymentMethod } from '../../../core/models/payment.model';
import { Reservation } from '../../../core/models/reservation.model';
import { Seat, SeatMap } from '../../../core/models/seat.model';
import { Trip } from '../../../core/models/trip.model';
import { PaymentService } from '../../../core/services/payment.service';
import { ReservationService } from '../../../core/services/reservation.service';
import { TripService } from '../../../core/services/trip.service';

type Step = 'count' | 'seats' | 'confirm' | 'payment' | 'pending';

const MAX_SEATS_PER_RESERVATION = 10;

@Component({
  selector: 'app-booking',
  imports: [DecimalPipe, DatePipe, RouterLink],
  templateUrl: './booking.html',
  styleUrl: './booking.scss',
})
export class Booking {
  private readonly route = inject(ActivatedRoute);
  private readonly tripService = inject(TripService);
  private readonly reservationService = inject(ReservationService);
  private readonly paymentService = inject(PaymentService);

  protected readonly trip = signal<Trip | null>(null);
  protected readonly reservation = signal<Reservation | null>(null);
  protected readonly loading = signal(true);
  protected readonly step = signal<Step>('count');
  protected readonly isResuming = signal(false);
  protected readonly error = signal('');

  protected readonly seatCount = signal(1);
  protected readonly selectedSeats = signal<number[]>([]);
  protected readonly seatMap = signal<SeatMap | null>(null);
  protected readonly loadingSeats = signal(false);
  protected readonly creatingReservation = signal(false);
  protected readonly payingMethod = signal<PaymentMethod | null>(null);
  protected readonly pendingPaymentId = signal<number | null>(null);
  protected readonly changingMethod = signal(false);
  protected readonly selectedMethod = signal<PaymentMethod | null>(null);
  protected readonly transactionReference = signal('');
  protected readonly paidMethod = signal<PaymentMethod | null>(null);

  protected readonly methods: { value: PaymentMethod; label: string }[] = [
    { value: 'Bankily', label: 'Bankily' },
    { value: 'Masrivi', label: 'Masrivi' },
    { value: 'CarteBancaire', label: 'Carte bancaire' },
    { value: 'Cash', label: 'Espèces' },
  ];

  protected readonly maxSeats = computed(() =>
    Math.min(this.trip()?.availableSeats ?? 1, MAX_SEATS_PER_RESERVATION),
  );

  protected readonly seats = computed<Seat[]>(() => this.seatMap()?.seats ?? []);

  protected readonly totalPrice = computed(() => (this.trip()?.price ?? 0) * this.seatCount());

  protected readonly paidMethodLabel = computed(
    () => this.methods.find((m) => m.value === this.paidMethod())?.label ?? '',
  );

  constructor() {
    const tripId = Number(this.route.snapshot.paramMap.get('id'));
    const reservationId = this.route.snapshot.queryParamMap.get('reservationId');

    this.tripService.getById(tripId).subscribe({
      next: (trip) => {
        this.trip.set(trip);
        this.loading.set(false);
        if (reservationId) {
          this.resumePayment(Number(reservationId));
        }
      },
      error: () => this.loading.set(false),
    });
  }

  private resumePayment(reservationId: number): void {
    this.isResuming.set(true);
    this.reservationService.getById(reservationId).subscribe({
      next: (reservation) => {
        this.reservation.set(reservation);
        if (reservation.paymentStatus === 'Pending' && reservation.paymentId) {
          // A payment already exists for this reservation and is awaiting admin verification.
          this.pendingPaymentId.set(reservation.paymentId);
          this.paidMethod.set(reservation.paymentMethod);
          this.step.set('pending');
        } else {
          this.step.set('payment');
        }
      },
      error: (err) => this.error.set(extractApiErrorMessage(err, 'Impossible de charger la réservation.')),
    });
  }

  protected setSeatCount(count: number): void {
    this.seatCount.set(Math.min(Math.max(1, count), this.maxSeats()));
  }

  protected goToSeats(): void {
    const trip = this.trip();
    if (!trip) return;

    this.selectedSeats.set([]);
    this.step.set('seats');
    this.loadingSeats.set(true);
    this.error.set('');
    this.tripService.getSeats(trip.id).subscribe({
      next: (seatMap) => {
        this.seatMap.set(seatMap);
        this.loadingSeats.set(false);
      },
      error: (err) => {
        this.loadingSeats.set(false);
        this.error.set(extractApiErrorMessage(err, 'Impossible de charger la carte des sièges.'));
      },
    });
  }

  protected toggleSeat(seat: Seat): void {
    if (seat.status !== 'Available') return;

    this.selectedSeats.update((seats) => {
      if (seats.includes(seat.seatNumber)) {
        return seats.filter((s) => s !== seat.seatNumber);
      }
      if (seats.length >= this.seatCount()) {
        return seats;
      }
      return [...seats, seat.seatNumber].sort((a, b) => a - b);
    });
  }

  protected goToConfirm(): void {
    this.step.set('confirm');
  }

  protected confirmReservation(): void {
    const trip = this.trip();
    if (!trip) return;

    this.creatingReservation.set(true);
    this.error.set('');
    this.reservationService.create({ tripId: trip.id, seatNumbers: this.selectedSeats() }).subscribe({
      next: (reservation) => {
        this.reservation.set(reservation);
        this.creatingReservation.set(false);
        this.step.set('payment');
      },
      error: (err) => {
        this.creatingReservation.set(false);
        const message = extractApiErrorMessage(err, 'La réservation a échoué.');
        // Un autre voyageur a pu prendre le siège entre-temps : on recharge la carte et on revient au choix des sièges.
        this.goToSeats();
        this.error.set(message);
      },
    });
  }

  /** Cash needs no external reference (paid in person); digital methods require the traveler to enter the reference they received after paying outside the app. */
  protected selectMethod(method: PaymentMethod): void {
    if (method === 'Cash') {
      this.pay(method, `CASH-${this.reservation()!.id}-${Date.now()}`);
      return;
    }
    this.selectedMethod.set(method);
    this.transactionReference.set('');
    this.error.set('');
  }

  protected cancelMethodSelection(): void {
    this.selectedMethod.set(null);
    this.transactionReference.set('');
  }

  protected confirmDigitalPayment(): void {
    const method = this.selectedMethod();
    const reference = this.transactionReference().trim();
    if (!method || !reference) return;
    this.pay(method, reference);
  }

  private pay(method: PaymentMethod, transactionId: string): void {
    const reservation = this.reservation();
    if (!reservation) return;

    this.payingMethod.set(method);
    this.error.set('');

    this.paymentService.create({ reservationId: reservation.id, method, transactionId }).subscribe({
      next: (payment) => {
        this.payingMethod.set(null);
        this.selectedMethod.set(null);
        this.pendingPaymentId.set(payment.id);
        this.paidMethod.set(method);
        this.step.set('pending');
      },
      error: (err) => {
        this.payingMethod.set(null);
        this.error.set(extractApiErrorMessage(err, 'Le paiement a échoué.'));
      },
    });
  }

  /** Cancels the pending payment so the traveler can pick a different payment method. */
  protected changePaymentMethod(): void {
    const paymentId = this.pendingPaymentId();
    if (!paymentId) return;

    this.changingMethod.set(true);
    this.error.set('');
    this.paymentService.cancel(paymentId).subscribe({
      next: () => {
        this.pendingPaymentId.set(null);
        this.paidMethod.set(null);
        this.changingMethod.set(false);
        this.step.set('payment');
      },
      error: (err) => {
        this.changingMethod.set(false);
        this.error.set(extractApiErrorMessage(err, "Impossible de changer le mode de paiement."));
      },
    });
  }
}
