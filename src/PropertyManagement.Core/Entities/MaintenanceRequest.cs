using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyManagement.Core.Entities
{
    public class MaintenanceRequest
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string RequestNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        public MaintenanceCategory Category { get; set; }
        
        public MaintenancePriority Priority { get; set; }
        
        public MaintenanceStatus Status { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? EstimatedCost { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ActualCost { get; set; }
        
        [StringLength(3)]
        public string Currency { get; set; } = "EGP";
        
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? ScheduledDate { get; set; }
        
        public DateTime? CompletedDate { get; set; }
        
        [StringLength(100)]
        public string AssignedTo { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string AssignedToPhone { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string CompletionNotes { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string InternalNotes { get; set; } = string.Empty;
        
        // Navigation Properties
        public int PropertyId { get; set; }
        public Property Property { get; set; } = null!;
        
        public int? TenantId { get; set; }
        public Tenant? Tenant { get; set; }
        
        public ICollection<MaintenanceImage> Images { get; set; } = new List<MaintenanceImage>();
        
        [NotMapped]
        public bool IsUrgent => Priority == MaintenancePriority.Emergency || Priority == MaintenancePriority.High;
        
        [NotMapped]
        public int DaysSinceRequest => (DateTime.UtcNow - RequestDate).Days;
    }
    
    public enum MaintenanceCategory
    {
        Plumbing = 1,
        Electrical = 2,
        HVAC = 3,
        Painting = 4,
        Flooring = 5,
        Appliances = 6,
        Security = 7,
        Cleaning = 8,
        Landscaping = 9,
        Structural = 10,
        Other = 11
    }
    
    public enum MaintenancePriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Emergency = 4
    }
    
    public enum MaintenanceStatus
    {
        Submitted = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4,
        OnHold = 5
    }
}
