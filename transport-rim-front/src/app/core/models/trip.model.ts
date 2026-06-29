export interface Trip {
  id: number;
  busId: number;
  busNumber: string;
  companyId: number;
  companyName: string;
  departureCity: string;
  arrivalCity: string;
  date: string;
  time: string;
  price: number;
  availableSeats: number;
}

export interface TripSearchParams {
  departureCity?: string;
  arrivalCity?: string;
  date?: string;
}

export interface CreateTripRequest {
  busId: number;
  departureCity: string;
  arrivalCity: string;
  date: string;
  time: string;
  price: number;
  availableSeats: number;
}

export interface UpdateTripRequest {
  departureCity: string;
  arrivalCity: string;
  date: string;
  time: string;
  price: number;
  availableSeats: number;
}
