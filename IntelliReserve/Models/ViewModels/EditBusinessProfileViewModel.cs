using System.ComponentModel.DataAnnotations;

public class EditBusinessProfileViewModel
{
    // User information
    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; }

    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string? Password { get; set; }

    // Business information
    [Required(ErrorMessage = "Organization name is required.")]
    public string OrganizationName { get; set; }

    [Required(ErrorMessage = "Address is required.")]
    public string Address { get; set; }

    [Required(ErrorMessage = "Phone number is required.")]
    public string Phone { get; set; }

    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; }
    
    public int OwnerId { get; set; }

}
