import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Bus } from '../../../core/models/bus.model';
import { BusService } from '../../../core/services/bus.service';
import { extractApiErrorMessage } from '../../../core/utils/api-error';

@Component({
  selector: 'app-company-buses',
  imports: [FormsModule],
  templateUrl: './buses.html',
  styleUrl: './buses.scss',
})
export class Buses {
  private readonly busService = inject(BusService);

  protected readonly buses = signal<Bus[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal('');
  protected readonly savingId = signal<number | 'new' | null>(null);

  protected readonly showForm = signal(false);
  protected readonly editingBus = signal<Bus | null>(null);
  protected readonly formBusNumber = signal('');
  protected readonly formCapacity = signal(50);

  constructor() {
    this.busService.getAll().subscribe({
      next: (buses) => {
        this.buses.set(buses);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  protected openCreateForm(): void {
    this.editingBus.set(null);
    this.formBusNumber.set('');
    this.formCapacity.set(50);
    this.showForm.set(true);
  }

  protected openEditForm(bus: Bus): void {
    this.editingBus.set(bus);
    this.formBusNumber.set(bus.busNumber);
    this.formCapacity.set(bus.capacity);
    this.showForm.set(true);
  }

  protected cancelForm(): void {
    this.showForm.set(false);
  }

  protected submitForm(): void {
    this.error.set('');
    const editing = this.editingBus();
    const payload = { busNumber: this.formBusNumber(), capacity: this.formCapacity() };

    if (editing) {
      this.savingId.set(editing.id);
      this.busService.update(editing.id, payload).subscribe({
        next: (updated) => {
          this.buses.update((list) => list.map((b) => (b.id === editing.id ? updated : b)));
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

    this.savingId.set('new');
    this.busService.create(payload).subscribe({
      next: (created) => {
        this.buses.update((list) => [...list, created]);
        this.savingId.set(null);
        this.showForm.set(false);
      },
      error: (err) => {
        this.savingId.set(null);
        this.error.set(extractApiErrorMessage(err, 'La création a échoué.'));
      },
    });
  }

  protected deleteBus(bus: Bus): void {
    if (!confirm(`Supprimer le bus ${bus.busNumber} ?`)) {
      return;
    }

    this.savingId.set(bus.id);
    this.busService.delete(bus.id).subscribe({
      next: () => {
        this.buses.update((list) => list.filter((b) => b.id !== bus.id));
        this.savingId.set(null);
      },
      error: (err) => {
        this.savingId.set(null);
        this.error.set(extractApiErrorMessage(err, 'La suppression a échoué.'));
      },
    });
  }
}
