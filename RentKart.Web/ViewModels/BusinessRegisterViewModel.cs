using System.ComponentModel.DataAnnotations;

namespace RentKart.Web.ViewModels;

public class BusinessRegisterViewModel
{
    [Required]
    [Display(Name = "Owner Name")]
    public string OwnerName { get; set; } = null!;

    [Required]
    [Display(Name = "Business Name")]
    public string BusinessName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [Phone]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    public string Address { get; set; } = null!;

    [Required]
    public string City { get; set; } = null!;

    [Required]
    public string State { get; set; } = null!;

    [Required]
    [Display(Name = "Postal Code")]
    public string PostalCode { get; set; } = null!;

    public string? Description { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = null!;
}
