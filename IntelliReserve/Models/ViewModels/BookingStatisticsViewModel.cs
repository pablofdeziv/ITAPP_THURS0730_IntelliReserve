namespace IntelliReserve.Models.ViewModels
{
    public class BookingStatisticsViewModel
    {
        public List<string> Labels { get; set; } = new();  // Meses (Ene, Feb, ...)
        public List<int> MonthlyCounts { get; set; } = new(); // Nº de reservas por mes
        public int TotalBookings { get; set; }
        public int ActiveBookings { get; set; }
        public int CancelledBookings { get; set; }
    }
}
