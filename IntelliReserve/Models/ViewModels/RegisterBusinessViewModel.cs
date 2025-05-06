using IntelliReserve.Models;
using System.ComponentModel.DataAnnotations;

public class RegisterBusinessViewModel
{
    // Datos del usuario
    [Required]
    public string Name { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }

    [Required, DataType(DataType.Password)]
    public string Password { get; set; }
    public int OwnerId { get; set; }

    public UserRole? Role { get; set; }



    // Datos del negocio
    [Required]
    public string OrganizationName { get; set; }

    [Required]
    public string Address { get; set; }

    [Required]
    public string Phone { get; set; }

    [Required]
    public string Description { get; set; }
}
