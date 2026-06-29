import { DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { AdminDashboard } from '../../../core/models/dashboard.model';
import { DashboardService } from '../../../core/services/dashboard.service';

@Component({
  selector: 'app-admin-dashboard',
  imports: [DecimalPipe],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard {
  private readonly dashboardService = inject(DashboardService);

  protected readonly dashboard = signal<AdminDashboard | null>(null);
  protected readonly loading = signal(true);

  constructor() {
    this.dashboardService.getAdminDashboard().subscribe({
      next: (data) => {
        this.dashboard.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}
