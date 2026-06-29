import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Company, CompanyStatus, CreateCompanyRequest } from '../models/company.model';

@Injectable({ providedIn: 'root' })
export class CompanyService {
  constructor(private readonly http: HttpClient) {}

  getAll() {
    return this.http.get<Company[]>(`${environment.apiUrl}/companies`);
  }

  create(request: CreateCompanyRequest) {
    return this.http.post<Company>(`${environment.apiUrl}/companies`, request);
  }

  getById(id: number) {
    return this.http.get<Company>(`${environment.apiUrl}/companies/${id}`);
  }

  updateStatus(id: number, status: CompanyStatus) {
    return this.http.put<void>(`${environment.apiUrl}/companies/${id}/status`, { status });
  }
}
