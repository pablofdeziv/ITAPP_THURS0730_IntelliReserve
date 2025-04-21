using System;

namespace IntelliReserve.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        public Guid BusinessId { get; set; }
        public int DayOfWeek { get; set; } // 0 = Sunday, 1 = Monday, ...
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }

        public Business Business { get; set; }
    }
}
