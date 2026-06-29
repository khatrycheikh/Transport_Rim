import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { Bus } from '../../../core/models/bus.model';
import { Company } from '../../../core/models/company.model';
import { BusService } from '../../../core/services/bus.service';
import { CompanyService } from '../../../core/services/company.service';
import { extractApiErrorMessage } from '../../../core/utils/api-error';

@Component({
  selector: 'app-admin-buses',
  imports: [FormsModule],
  templateUrl: './buses.html',
  styleUrl: './buses.scss',
})
export class Buses {
  private readonly busService = inject(BusService);
  private readonly companyService = inject(CompanyService);

  protected readonly buses = signal<Bus[]>([]);
  protected readonly companies = signal<Company[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal('');
  protected readonly savingId = signal<number | 'new' | null>(null);

  protected readonly showForm = signal(false);
  protected readonly editingBus = signal<Bus | null>(null);
  protected readonly formBusNumber = signal('');
  protected readonly formCapacity = signal(50);
  protected readonly formCompanyId = signal<number | null>(null);

  constructor() {
    forkJoin({ buses: this.busService.getAll(), companies: this.companyService.getAll() }).subscribe({
      next: ({ buses, companies }) => {
        this.buses.set(buses);
        this.companies.set(companies);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  protected openCreateForm(): void {
    this.editingBus.set(null);
    this.formBusNumber.set('');
    this.formCapacity.set(50);
    this.formCompanyId.set(this.companies()[0]?.id ?? null);
    this.showForm.set(true);
  }

  protected openEditForm(bus: Bus): void {
    this.editingBus.set(bus);
    this.formBusNumber.set(bus.busNumber);
    this.formCapacity.set(bus.capacity);
    this.formCompanyId.set(bus.companyId);
    this.showForm.set(true);
  }

  protected cancelForm(): void {
    this.showForm.set(false);
  }

  protected submitForm(): void {
    this.error.set('');
    const editing = this.editingBus();

    if (editing) {
      const companyId = this.formCompanyId();
      if (!companyId) {
        this.error.set('Veuillez sélectionner une compagnie.');
        return;
      }

      this.savingId.set(editing.id);
      this.busService.update(editing.id, { busNumber: this.formBusNumber(), capacity: this.formCapacity(), companyId }).subscribe({
        next: (updated) => {
          this.buses.update((list) => list.map((b) => (b.id === editing.id ? updated : b)));
          this.savingId.set(null);
          this.showForm.set(false);
        },
        error: (err) => {
          this.savingId.set(null);
          this.error.set(extractApiErrorMessage(err, "La modification a échoué."));
        },
      });
      return;
    }

    const companyId = this.formCompanyId();
    if (!companyId) {
      this.error.set('Veuillez sélectionner une compagnie.');
      return;
    }

    this.savingId.set('new');
    this.busService.create({ busNumber: this.formBusNumber(), capacity: this.formCapacity(), companyId }).subscribe({
      next: (created) => {
        this.buses.update((list) => [...list, created]);
        this.savingId.set(null);
        this.showForm.set(false);
      },
      error: (err) => {
        this.savingId.set(null);
        this.error.set(extractApiErrorMessage(err, "La création a échoué."));
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
