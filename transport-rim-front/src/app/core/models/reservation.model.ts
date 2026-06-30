import { PaymentMethod, PaymentStatus } from './payment.model';

export type ReservationStatus = 'Pending' | 'Confirmed' | 'Cancelled';

export interface Reservation {
  id: number;
  userId: number;
  userName: string;
  tripId: number;
  departureCity: string;
  arrivalCity: string;
  tripDate: string;
  tripPrice: number;
  reservedSeats: number;
  totalPrice: number;
  status: ReservationStatus;
  createdAt: string;
  paymentId: number | null;
  paymentMethod: PaymentMethod | null;
  paymentStatus: PaymentStatus | null;
  paymentTransactionId: string | null;
}

export interface CreateReservationRequest {
  tripId: number;
  reservedSeats: number;
}

export interface UpdateReservationStatusRequest {
  status: ReservationStatus;
}
