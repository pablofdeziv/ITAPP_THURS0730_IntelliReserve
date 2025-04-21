using System;

namespace IntelliReserve.Models
{
    public class AppointmentHistory
    {
        public int Id { get; set; }
        public Guid AppointmentId { get; set; }
        public DateTime Timestamp { get; set; }
        public string StatusChange { get; set; }
        public string Comment { get; set; }

        public Appointment Appointment { get; set; }
    }
}
