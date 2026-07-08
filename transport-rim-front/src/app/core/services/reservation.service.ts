import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateReservationRequest, Reservation, ReservationStatus } from '../models/reservation.model';
import { AuthService } from './auth.service';

const STORAGE_KEY = 'transport-rim-my-reservations';

/**
 * The API has no "list my reservations" endpoint, so reservation ids created by the
 * current user are tracked client-side and re-fetched individually via getById.
 */
@Injectable({ providedIn: 'root' })
export class ReservationService {
  constructor(
    private readonly http: HttpClient,
    private readonly auth: AuthService,
  ) {}

  create(request: CreateReservationRequest) {
    return this.http
      .post<Reservation>(`${environment.apiUrl}/reservations`, request)
      .pipe(tap((reservation) => this.rememberId(reservation.id)));
  }

  getById(id: number) {
    return this.http.get<Reservation>(`${environment.apiUrl}/reservations/${id}`);
  }

  cancel(id: number) {
    return this.http
      .delete<void>(`${environment.apiUrl}/reservations/${id}`)
      .pipe(tap(() => this.forgetId(id)));
  }

  /** Admin: every reservation across all travelers. Company: only reservations for its own trips. */
  getAll() {
    return this.http.get<Reservation[]>(`${environment.apiUrl}/reservations`);
  }

  /** Admin or Company (own trips only): set a reservation's status, e.g. "Valider" -> Confirmed. */
  setStatus(id: number, status: ReservationStatus) {
    return this.http.put<Reservation>(`${environment.apiUrl}/reservations/${id}/status`, { status });
  }

  myReservationIds(): number[] {
    const all = this.readStore();
    const userId = this.auth.currentUser()?.id;
    return userId !== undefined ? all[userId] ?? [] : [];
  }

  private rememberId(id: number): void {
    const userId = this.auth.currentUser()?.id;
    if (userId === undefined) return;

    const all = this.readStore();
    const ids = all[userId] ?? [];
    all[userId] = ids.includes(id) ? ids : [...ids, id];
    this.writeStore(all);
  }

  private forgetId(id: number): void {
    const userId = this.auth.currentUser()?.id;
    if (userId === undefined) return;

    const all = this.readStore();
    all[userId] = (all[userId] ?? []).filter((existingId) => existingId !== id);
    this.writeStore(all);
  }

  private readStore(): Record<number, number[]> {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? JSON.parse(raw) : {};
  }

  private writeStore(value: Record<number, number[]>): void {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(value));
  }
}
