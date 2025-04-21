using System;

namespace IntelliReserve.Models
{
    public class Review
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public Guid BusinessId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; }
        public Business Business { get; set; }
    }
}
