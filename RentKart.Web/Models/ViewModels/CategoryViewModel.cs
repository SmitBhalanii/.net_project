using System.ComponentModel.DataAnnotations;

namespace RentKart.Web.Models.ViewModels;

public class CategoryViewModel
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = null!;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; }
}
