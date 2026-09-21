using System;

namespace RentKart.Core.Entities;

public class BookingItem
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public int EquipmentId { get; set; }
    public Equipment Equipment { get; set; } = null!;

    public int Quantity { get; set; }
    public decimal RentalPricePerDay { get; set; }
    public int NumberOfDays { get; set; }
    public decimal Subtotal { get; set; }
    public DateTime CreatedAt { get; set; }
}
