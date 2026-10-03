using System.ComponentModel.DataAnnotations;

namespace RentKart.Web.Models.ViewModels;

public class StaffViewModel
{
    public string? Id { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    
    [Required]
    public string FirstName { get; set; } = null!;
    
    [Required]
    public string LastName { get; set; } = null!;
    
    public bool IsActive { get; set; } = true;
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = null!;
}
