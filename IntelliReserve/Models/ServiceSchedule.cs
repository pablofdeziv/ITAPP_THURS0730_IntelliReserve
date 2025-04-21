using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntelliReserve.Models
{
    public class ServiceSchedule
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }

        [ForeignKey("ServiceId")]
        public Service Service { get; set; } = null!;
    }
}
