using System;
using System.Collections.Generic;

namespace IntelliReserve.Models
{
    public enum AppointmentStatus { Pending, Confirmed, Canceled }

    public class Appointment
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ServiceId { get; set; }
        public Guid EmployeeId { get; set; }
        public DateTime DateTime { get; set; }
        public AppointmentStatus Status { get; set; }

        public User User { get; set; }
        public Service Service { get; set; }
        public Employee Employee { get; set; }

        public ICollection<AppointmentHistory> History { get; set; }
        public Payment? Payment { get; set; } // nullable, ya que puede no tener pago aún
    }
}
