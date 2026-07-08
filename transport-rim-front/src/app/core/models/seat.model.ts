export type SeatStatus = 'Available' | 'Pending' | 'Confirmed';

export interface Seat {
  seatNumber: number;
  status: SeatStatus;
}

export interface SeatMap {
  capacity: number;
  seats: Seat[];
}
