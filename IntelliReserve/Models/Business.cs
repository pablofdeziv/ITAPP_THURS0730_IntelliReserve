using System;
using System.Collections.Generic;

namespace IntelliReserve.Models
{
    public class Business
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int OwnerId { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Description { get; set; }

        public User Owner { get; set; }
        //public ICollection<Employee> Employees { get; set; }
        public ICollection<Service> Services { get; set; }
        //public ICollection<Schedule> Schedules { get; set; }
        //public ICollection<Review> Reviews { get; set; }
    }
}
