using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyManagement.Core.Entities
{
    public class PropertyImage
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
        
        public int DisplayOrder { get; set; }
        
        public bool IsPrimary { get; set; }
        
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation Properties
        public int PropertyId { get; set; }
        public Property Property { get; set; } = null!;
    }
}
