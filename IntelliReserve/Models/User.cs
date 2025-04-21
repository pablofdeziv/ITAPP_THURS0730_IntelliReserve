using System;
using System.Collections.Generic;

namespace IntelliReserve.Models
{
    public enum UserRole { Customer, BusinessAdmin }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; }

        public ICollection<Notification> Notifications { get; set; }
        public ICollection<Business> Businesses { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}
