import { Company } from './company.model';
import { Reservation } from './reservation.model';

export interface Payment {
  id: number;
  reservationId: number;
  amount: number;
  method: string;
  transactionId: string;
  status: string;
  createdAt: string;
}

export interface CompanyDashboard {
  totalBuses: number;
  totalTrips: number;
  totalReservations: number;
  confirmedReservationsCount: number;
  totalSeatsBooked: number;
  totalRevenue: number;
  averageBusOccupancy: number;
  recentReservations: Reservation[];
  recentPayments: Payment[];
}

export interface AdminDashboard {
  totalCompanies: number;
  activeCompaniesCount: number;
  pendingCompaniesCount: number;
  suspendedCompaniesCount: number;
  totalUsersCount: number;
  travelersCount: number;
  companyManagersCount: number;
  adminsCount: number;
  totalBusesCount: number;
  totalTripsCount: number;
  totalReservationsCount: number;
  globalRevenue: number;
  recentCompanies: Company[];
  recentPayments: Payment[];
}
