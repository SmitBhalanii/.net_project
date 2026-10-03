using System;
using RentKart.Core.Enums;

namespace RentKart.Core.Entities;

public class Report
{
    public int Id { get; set; }
    public string ReporterUserId { get; set; } = null!;
    public ApplicationUser Reporter { get; set; } = null!;
    
    public ReportTargetType TargetType { get; set; }
    public string TargetId { get; set; } = null!; // String to handle both int (EquipmentId) and string (UserId)
    
    public string Reason { get; set; } = null!;
    public string Description { get; set; } = null!;
    
    public ReportStatus Status { get; set; }
    public string? ResolutionNotes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedByUserId { get; set; }
    public ApplicationUser? ResolvedBy { get; set; }
}
