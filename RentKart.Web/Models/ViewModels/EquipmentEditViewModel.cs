using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using RentKart.Core.Enums;
using RentKart.Core.Entities;

namespace RentKart.Web.Models.ViewModels;

public class EquipmentEditViewModel
{
    public int Id { get; set; }
    
    [Required]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }
    
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Name { get; set; } = null!;
    
    [Required]
    [StringLength(100)]
    public string Brand { get; set; } = null!;
    
    [Required]
    [StringLength(100)]
    public string Model { get; set; } = null!;
    
    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = null!;
    
    [Required]
    [Range(1, 1000000)]
    [Display(Name = "Rental Price")]
    public decimal RentalPrice { get; set; }
    
    [Required]
    [Display(Name = "Rental Period")]
    public RentalPeriod RentalPeriod { get; set; }
    
    [Required]
    [Range(0, 1000000)]
    [Display(Name = "Security Deposit")]
    public decimal SecurityDeposit { get; set; }
    
    [Required]
    [Range(1, 10000)]
    public int Quantity { get; set; } = 1;
    
    [Required]
    public EquipmentCondition Condition { get; set; }
    
    [Required]
    public EquipmentStatus Status { get; set; }
    
    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;
    
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

    public List<IFormFile>? NewImages { get; set; }
    
    public List<EquipmentImage> ExistingImages { get; set; } = new();
    
    public List<int>? ImagesToDelete { get; set; }
}
