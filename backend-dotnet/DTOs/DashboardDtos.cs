using System;
using System.Collections.Generic;

namespace TransportRim.Api.DTOs
{
    /// <summary>
    /// DTO contenant les indicateurs clés et l'activité récente pour une compagnie spécifique.
    /// </summary>
    public class CompanyDashboardDto
    {
        public int TotalBuses { get; set; }
        public int TotalTrips { get; set; }
        public int TotalReservations { get; set; }
        public int ConfirmedReservationsCount { get; set; }
        public int TotalSeatsBooked { get; set; }
        public decimal TotalRevenue { get; set; }
        public double AverageBusOccupancy { get; set; }
        
        public List<ReservationDto> RecentReservations { get; set; } = new();
        public List<PaymentDto> RecentPayments { get; set; } = new();
    }

    /// <summary>
    /// DTO contenant les indicateurs clés et l'activité récente globale de la plateforme pour l'Administrateur.
    /// </summary>
    public class AdminDashboardDto
    {
        // Compagnies
        public int TotalCompanies { get; set; }
        public int ActiveCompaniesCount { get; set; }
        public int PendingCompaniesCount { get; set; }
        public int SuspendedCompaniesCount { get; set; }

        // Utilisateurs
        public int TotalUsersCount { get; set; }
        public int TravelersCount { get; set; }
        public int CompanyManagersCount { get; set; }
        public int AdminsCount { get; set; }

        // Système
        public int TotalBusesCount { get; set; }
        public int TotalTripsCount { get; set; }
        public int TotalReservationsCount { get; set; }
        public decimal GlobalRevenue { get; set; }

        // Activités récentes
        public List<CompanyDto> RecentCompanies { get; set; } = new();
        public List<PaymentDto> RecentPayments { get; set; } = new();
    }
}
