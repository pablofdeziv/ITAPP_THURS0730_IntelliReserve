using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntelliReserve.Models
{
    public enum PaymentStatus { Pending, Paid, Refunded }

    public class Payment
    {
        public int Id { get; set; }

        // FK
        public int AppointmentId { get; set; }

        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentStatus Status { get; set; }

        [ForeignKey("AppointmentId")]
        public Appointment Appointment { get; set; } = null!;
    }
}
