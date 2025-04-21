using System;

namespace IntelliReserve.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public Guid BusinessId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }

        public Business Business { get; set; }
    }
}
