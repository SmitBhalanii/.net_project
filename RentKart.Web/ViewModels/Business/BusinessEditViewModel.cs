using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RentKart.Web.ViewModels.Business;

public class BusinessEditViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 3)]
    [Display(Name = "Business Name")]
    public string BusinessName { get; set; } = null!;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    [StringLength(250)]
    public string Address { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string State { get; set; } = null!;

    [Required]
    [StringLength(20)]
    [Display(Name = "Postal Code")]
    public string PostalCode { get; set; } = null!;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    [Display(Name = "Contact Email")]
    public string ContactEmail { get; set; } = null!;

    [Required]
    [Phone]
    [StringLength(20)]
    [Display(Name = "Contact Phone")]
    public string ContactPhone { get; set; } = null!;

    [Url]
    [StringLength(250)]
    public string? Website { get; set; }

    public string? LogoPath { get; set; }

    [Display(Name = "Business Logo")]
    public IFormFile? LogoFile { get; set; }
}
