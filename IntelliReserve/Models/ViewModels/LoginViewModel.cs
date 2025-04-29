using System.ComponentModel.DataAnnotations;

namespace IntelliReserve.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        [EmailAddress(ErrorMessage = "Por favor, introduce un correo electrónico válido.")]
        [Display(Name = "Correo electrónico")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string? Password { get; set; }

    }
}
