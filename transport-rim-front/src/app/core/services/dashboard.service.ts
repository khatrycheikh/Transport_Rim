import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { AdminDashboard, CompanyDashboard } from '../models/dashboard.model';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  constructor(private readonly http: HttpClient) {}

  getAdminDashboard() {
    return this.http.get<AdminDashboard>(`${environment.apiUrl}/dashboard/admin`);
  }

  getCompanyDashboard() {
    return this.http.get<CompanyDashboard>(`${environment.apiUrl}/dashboard/company`);
  }
}
