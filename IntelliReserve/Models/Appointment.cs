using System;
using System.Collections.Generic;

namespace IntelliReserve.Models
{
    public enum AppointmentStatus { Pending,Completed, Confirmed, Canceled }

    public class Appointment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ServiceScheduleId { get; set; }
        public ServiceSchedule ServiceSchedule { get; set; }
        public AppointmentStatus Status { get; set; }

        public User User { get; set; }

       public ICollection<AppointmentHistory> History { get; set; }
    }
}
