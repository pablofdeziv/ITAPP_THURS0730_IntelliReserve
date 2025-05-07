using System;

namespace IntelliReserve.Models
{
    public class Service
    {
        public int Id { get; set; }
        public int BusinessId { get; set; }
        public string Name { get; set; }
        public int Duration { get; set; }
        public decimal Price { get; set; }

        public Business Business { get; set; }
    }
}
