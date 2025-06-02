namespace IntelliReserve.Models
{
    public class ServiceAvailability
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }

        public Service Service { get; set; }
    }

}
