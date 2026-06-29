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
}

export interface CreateReservationRequest {
  tripId: number;
  reservedSeats: number;
}

export interface UpdateReservationStatusRequest {
  status: ReservationStatus;
}
