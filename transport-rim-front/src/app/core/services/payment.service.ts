import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CreatePaymentRequest, Payment, PaymentStatus } from '../models/payment.model';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  constructor(private readonly http: HttpClient) {}

  create(request: CreatePaymentRequest) {
    return this.http.post<Payment>(`${environment.apiUrl}/payments`, request);
  }

  getByReservationId(reservationId: number) {
    return this.http.get<Payment>(`${environment.apiUrl}/payments/reservation/${reservationId}`);
  }

  /** Admin or Company (own trips only): validates a pending payment (e.g. cash received at the agency), which confirms the reservation and generates the ticket. */
  updateStatus(id: number, status: PaymentStatus) {
    return this.http.put<Payment>(`${environment.apiUrl}/payments/${id}/status`, JSON.stringify(status), {
      headers: { 'Content-Type': 'application/json' },
    });
  }

  /** Cancels a still-pending payment so the traveler can retry with a different method. */
  cancel(id: number) {
    return this.http.delete<void>(`${environment.apiUrl}/payments/${id}`);
  }
}
