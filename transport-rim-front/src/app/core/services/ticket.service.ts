import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Ticket } from '../models/ticket.model';

@Injectable({ providedIn: 'root' })
export class TicketService {
  constructor(private readonly http: HttpClient) {}

  getByReservationId(reservationId: number) {
    return this.http.get<Ticket>(`${environment.apiUrl}/tickets/reservation/${reservationId}`);
  }
}
