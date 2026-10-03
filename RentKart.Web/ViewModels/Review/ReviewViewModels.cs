using System.ComponentModel.DataAnnotations;

namespace RentKart.Web.ViewModels.Review;

public class CreateReviewViewModel
{
    [Required]
    public int RentalId { get; set; }
    
    [Required]
    public int BookingId { get; set; }

    [Required]
    public int EquipmentId { get; set; }

    [Required]
    public int BusinessId { get; set; }

    public string EquipmentName { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Equipment rating is required")]
    [Range(1, 5, ErrorMessage = "Equipment rating must be between 1 and 5")]
    [Display(Name = "Equipment Rating")]
    public int EquipmentRating { get; set; }

    [Required(ErrorMessage = "Business rating is required")]
    [Range(1, 5, ErrorMessage = "Business rating must be between 1 and 5")]
    [Display(Name = "Business Rating")]
    public int BusinessRating { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Comment must be between 10 and 1000 characters.")]
    [Display(Name = "Your Experience")]
    public string Comment { get; set; } = string.Empty;
}

public class EditReviewViewModel
{
    public int Id { get; set; }

    public string EquipmentName { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Equipment rating is required")]
    [Range(1, 5, ErrorMessage = "Equipment rating must be between 1 and 5")]
    [Display(Name = "Equipment Rating")]
    public int EquipmentRating { get; set; }

    [Required(ErrorMessage = "Business rating is required")]
    [Range(1, 5, ErrorMessage = "Business rating must be between 1 and 5")]
    [Display(Name = "Business Rating")]
    public int BusinessRating { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Comment must be between 10 and 1000 characters.")]
    [Display(Name = "Your Experience")]
    public string Comment { get; set; } = string.Empty;
}
