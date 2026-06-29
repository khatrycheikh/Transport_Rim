import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Bus, CreateBusRequest, UpdateBusRequest } from '../models/bus.model';

@Injectable({ providedIn: 'root' })
export class BusService {
  constructor(private readonly http: HttpClient) {}

  getAll() {
    return this.http.get<Bus[]>(`${environment.apiUrl}/buses`);
  }

  create(request: CreateBusRequest) {
    return this.http.post<Bus>(`${environment.apiUrl}/buses`, request);
  }

  update(id: number, request: UpdateBusRequest) {
    return this.http.put<Bus>(`${environment.apiUrl}/buses/${id}`, request);
  }

  delete(id: number) {
    return this.http.delete<void>(`${environment.apiUrl}/buses/${id}`);
  }
}
