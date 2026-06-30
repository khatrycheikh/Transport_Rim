export interface Ticket {
  id: number;
  reservationId: number;
  qrCodeData: string;
  generatedAt: string;
  departureCity: string;
  arrivalCity: string;
  tripDate: string;
  reservedSeats: number;
  totalPrice: number;
  passengerName: string;
  passengerPhone: string;
}
