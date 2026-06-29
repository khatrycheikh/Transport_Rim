import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CreateTripRequest, Trip, TripSearchParams, UpdateTripRequest } from '../models/trip.model';

@Injectable({ providedIn: 'root' })
export class TripService {
  constructor(private readonly http: HttpClient) {}

  search(params: TripSearchParams) {
    let httpParams = new HttpParams();
    if (params.departureCity) httpParams = httpParams.set('departureCity', params.departureCity);
    if (params.arrivalCity) httpParams = httpParams.set('arrivalCity', params.arrivalCity);
    if (params.date) httpParams = httpParams.set('date', params.date);

    return this.http.get<Trip[]>(`${environment.apiUrl}/trips/search`, { params: httpParams });
  }

  getById(id: number) {
    return this.http.get<Trip>(`${environment.apiUrl}/trips/${id}`);
  }

  getAll() {
    return this.http.get<Trip[]>(`${environment.apiUrl}/trips`);
  }

  create(request: CreateTripRequest) {
    return this.http.post<Trip>(`${environment.apiUrl}/trips`, request);
  }

  update(id: number, request: UpdateTripRequest) {
    return this.http.put<Trip>(`${environment.apiUrl}/trips/${id}`, request);
  }

  delete(id: number) {
    return this.http.delete<void>(`${environment.apiUrl}/trips/${id}`);
  }
}
