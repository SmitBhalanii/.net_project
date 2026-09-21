using System;
using Microsoft.AspNetCore.Identity;

namespace RentKart.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? ProfileImagePath { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}
