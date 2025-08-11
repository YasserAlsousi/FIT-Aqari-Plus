using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyManagement.Core.Entities
{
    public class MaintenanceImage
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(500)]
        public string FileName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string FilePath { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string ContentType { get; set; } = string.Empty;
        
        public long FileSize { get; set; }
        
        [StringLength(200)]
        public string Caption { get; set; } = string.Empty;
        
        public MaintenanceImageType Type { get; set; }
        
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation Properties
        public int MaintenanceRequestId { get; set; }
        public MaintenanceRequest MaintenanceRequest { get; set; } = null!;
    }
    
    public enum MaintenanceImageType
    {
        Before = 1,
        During = 2,
        After = 3,
        Invoice = 4,
        Other = 5
    }
}