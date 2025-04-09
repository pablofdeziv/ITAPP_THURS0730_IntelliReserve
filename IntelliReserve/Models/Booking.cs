using System;

namespace IntelliReserve.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public int ServiceId { get; set; }
        public Service Service { get; set; }

        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } // e.g., Confirmed, Cancelled, Pending
    }
}
