import { Component, computed, effect, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { Bus } from '../../../core/models/bus.model';
import { Trip } from '../../../core/models/trip.model';
import { BusService } from '../../../core/services/bus.service';
import { TripService } from '../../../core/services/trip.service';
import { extractApiErrorMessage } from '../../../core/utils/api-error';
import { MAURITANIA_CITIES } from '../../../core/constants/cities';

@Component({
  selector: 'app-company-trajets',
  imports: [FormsModule, DatePipe, DecimalPipe],
  templateUrl: './trajets.html',
  styleUrl: './trajets.scss',
})
export class Trajets {
  private readonly tripService = inject(TripService);
  private readonly busService = inject(BusService);

  protected readonly cities = MAURITANIA_CITIES;

  protected readonly trips = signal<Trip[]>([]);
  protected readonly buses = signal<Bus[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal('');
  protected readonly savingId = signal<number | 'new' | null>(null);

  protected readonly showForm = signal(false);
  protected readonly editingTrip = signal<Trip | null>(null);
  protected readonly formBusId = signal<number | null>(null);
  protected readonly formDepartureCity = signal('');
  protected readonly formArrivalCity = signal('');
  protected readonly formDate = signal('');
  protected readonly formTime = signal('');
  protected readonly formPrice = signal(500);
  protected readonly formAvailableSeats = signal(40);

  /** Capacity of the currently selected bus, or null while none is selected. */
  protected readonly selectedBusCapacity = computed(
    () => this.buses().find((bus) => bus.id === this.formBusId())?.capacity ?? null,
  );

  constructor() {
    forkJoin({ trips: this.tripService.getMine(), buses: this.busService.getAll() }).subscribe({
      next: ({ trips, buses }) => {
        this.trips.set(trips);
        this.buses.set(buses);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });

    // A new trip always starts with every seat of its bus available; keep the field in sync
    // with the bus picked so it can never be created above the bus's real capacity.
    effect(() => {
      const capacity = this.selectedBusCapacity();
      if (capacity !== null && !this.editingTrip()) {
        this.formAvailableSeats.set(capacity);
      }
    });
  }

  protected openCreateForm(): void {
    this.editingTrip.set(null);
    this.formBusId.set(this.buses()[0]?.id ?? null);
    this.formDepartureCity.set('');
    this.formArrivalCity.set('');
    this.formDate.set('');
    this.formTime.set('');
    this.formPrice.set(500);
    this.formAvailableSeats.set(40);
    this.showForm.set(true);
  }

  protected openEditForm(trip: Trip): void {
    this.editingTrip.set(trip);
    this.formBusId.set(trip.busId);
    this.formDepartureCity.set(trip.departureCity);
    this.formArrivalCity.set(trip.arrivalCity);
    this.formDate.set(trip.date.slice(0, 10));
    this.formTime.set(trip.time.slice(0, 5));
    this.formPrice.set(trip.price);
    this.formAvailableSeats.set(trip.availableSeats);
    this.showForm.set(true);
  }

  protected cancelForm(): void {
    this.showForm.set(false);
  }

  protected submitForm(): void {
    this.error.set('');
    const editing = this.editingTrip();

    const capacity = this.selectedBusCapacity();
    if (capacity !== null && this.formAvailableSeats() > capacity) {
      this.error.set(`Les places disponibles ne peuvent pas dépasser la capacité du bus (${capacity}).`);
      return;
    }

    const payload = {
      departureCity: this.formDepartureCity(),
      arrivalCity: this.formArrivalCity(),
      date: this.formDate(),
      time: this.formTime(),
      price: this.formPrice(),
      availableSeats: this.formAvailableSeats(),
    };

    if (editing) {
      this.savingId.set(editing.id);
      this.tripService.update(editing.id, payload).subscribe({
        next: (updated) => {
          this.trips.update((list) => list.map((t) => (t.id === editing.id ? updated : t)));
          this.savingId.set(null);
          this.showForm.set(false);
        },
        error: (err) => {
          this.savingId.set(null);
          this.error.set(extractApiErrorMessage(err, 'La modification a échoué.'));
        },
      });
      return;
    }

    const busId = this.formBusId();
    if (!busId) {
      this.error.set('Veuillez sélectionner un bus.');
      return;
    }

    this.savingId.set('new');
    this.tripService.create({ busId, ...payload }).subscribe({
      next: (created) => {
        this.trips.update((list) => [...list, created]);
        this.savingId.set(null);
        this.showForm.set(false);
      },
      error: (err) => {
        this.savingId.set(null);
        this.error.set(extractApiErrorMessage(err, 'La création a échoué.'));
      },
    });
  }

  protected deleteTrip(trip: Trip): void {
    if (!confirm(`Supprimer le trajet ${trip.departureCity} → ${trip.arrivalCity} ?`)) {
      return;
    }

    this.savingId.set(trip.id);
    this.tripService.delete(trip.id).subscribe({
      next: () => {
        this.trips.update((list) => list.filter((t) => t.id !== trip.id));
        this.savingId.set(null);
      },
      error: (err) => {
        this.savingId.set(null);
        this.error.set(extractApiErrorMessage(err, 'La suppression a échoué.'));
      },
    });
  }
}
