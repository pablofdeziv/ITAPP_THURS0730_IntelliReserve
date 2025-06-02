using System.ComponentModel.DataAnnotations;

public class EditBusinessProfileViewModel
{
 
    // User information
    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string? Password { get; set; }  // puede ser null, bien así

    // Business information
    [Required(ErrorMessage = "Organization name is required.")]
    public string OrganizationName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required.")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; } = string.Empty;

    public int OwnerId { get; set; }
}



