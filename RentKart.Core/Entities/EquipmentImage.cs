using System;

namespace RentKart.Core.Entities;

public class EquipmentImage
{
    public int Id { get; set; }
    public int EquipmentId { get; set; }
    public Equipment Equipment { get; set; } = null!;
    public string ImagePath { get; set; } = null!;
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
