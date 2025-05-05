using System.ComponentModel.DataAnnotations;
using IntelliReserve.Models; // Esto es necesario para UserRole

namespace IntelliReserve.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public UserRole? Role { get; set; }
    }
}

