import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Company, CompanyStatus } from '../../../core/models/company.model';
import { CompanyService } from '../../../core/services/company.service';
import { extractApiErrorMessage } from '../../../core/utils/api-error';

@Component({
  selector: 'app-admin-companies',
  imports: [FormsModule],
  templateUrl: './companies.html',
  styleUrl: './companies.scss',
})
export class Companies {
  private readonly companyService = inject(CompanyService);

  protected readonly companies = signal<Company[]>([]);
  protected readonly loading = signal(true);
  protected readonly search = signal('');
  protected readonly updatingId = signal<number | null>(null);
  protected readonly error = signal('');

  protected readonly showForm = signal(false);
  protected readonly saving = signal(false);
  protected readonly formName = signal('');
  protected readonly formPhone = signal('');
  protected readonly formEmail = signal('');
  protected readonly formAddress = signal('');

  protected readonly filteredCompanies = computed(() => {
    const term = this.search().toLowerCase();
    return this.companies().filter((company) => !term || company.name.toLowerCase().includes(term));
  });

  constructor() {
    this.companyService.getAll().subscribe({
      next: (companies) => {
        this.companies.set(companies);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  protected onSearchInput(event: Event): void {
    this.search.set((event.target as HTMLInputElement).value);
  }

  protected setStatus(company: Company, status: CompanyStatus): void {
    this.updatingId.set(company.id);
    this.companyService.updateStatus(company.id, status).subscribe({
      next: () => {
        this.companies.update((list) =>
          list.map((c) => (c.id === company.id ? { ...c, status } : c)),
        );
        this.updatingId.set(null);
      },
      error: () => this.updatingId.set(null),
    });
  }

  protected openCreateForm(): void {
    this.formName.set('');
    this.formPhone.set('');
    this.formEmail.set('');
    this.formAddress.set('');
    this.error.set('');
    this.showForm.set(true);
  }

  protected cancelForm(): void {
    this.showForm.set(false);
  }

  protected submitForm(): void {
    this.error.set('');
    this.saving.set(true);

    this.companyService
      .create({
        name: this.formName(),
        phone: this.formPhone(),
        email: this.formEmail(),
        address: this.formAddress(),
      })
      .subscribe({
        next: (created) => {
          this.companies.update((list) => [...list, created]);
          this.saving.set(false);
          this.showForm.set(false);
        },
        error: (err) => {
          this.saving.set(false);
          this.error.set(extractApiErrorMessage(err, "La création a échoué."));
        },
      });
  }
}
