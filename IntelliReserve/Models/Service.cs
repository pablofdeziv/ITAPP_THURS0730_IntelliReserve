using System;

namespace IntelliReserve.Models
{
    public class Service
    {
        public int Id { get; set; }
        public int BusinessId { get; set; }

        public string Name { get; set; }
        public int Duration { get; set; } // en minutos
        public decimal Price { get; set; }

        public DateTime AvailableFrom { get; set; }
        public DateTime AvailableTo { get; set; }

        public Business Business { get; set; }

        public ICollection<ServiceSchedule> Schedules { get; set; }
        public ICollection<ServiceAvailability> AvailableDays { get; set; }
    }

}
