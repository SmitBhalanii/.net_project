using System;
using RentKart.Core.Enums;

namespace RentKart.Core.Entities;

public class MaintenanceRecord
{
    public int Id { get; set; }
    public int EquipmentId { get; set; }
    public Equipment Equipment { get; set; } = null!;
    
    public string ReportedByUserId { get; set; } = null!;
    public string ProblemDescription { get; set; } = null!;
    
    public DateTime StartedAt { get; set; }
    public DateTime? ExpectedCompletionAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    public decimal? Cost { get; set; }
    public MaintenanceStatus Status { get; set; }
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
