using System.ComponentModel.DataAnnotations;
using IntelliReserve.Models;

namespace IntelliReserve.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Name is mandatory.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is mandatory.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is mandatory.")]
        [MinLength(6, ErrorMessage = "At least 6 carachters for the password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public UserRole? Role { get; set; }
    }
}
