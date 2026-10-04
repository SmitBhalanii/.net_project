using System.ComponentModel.DataAnnotations;
using RentKart.Core.Entities;

namespace RentKart.Core.Entities
{
    public class NotificationPreference
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;

        public bool BookingUpdates { get; set; } = true;
        public bool PaymentUpdates { get; set; } = true;
        public bool RentalReminders { get; set; } = true;
        public bool ReviewReminders { get; set; } = true;
        public bool MarketingPromotional { get; set; } = false;
    }
}