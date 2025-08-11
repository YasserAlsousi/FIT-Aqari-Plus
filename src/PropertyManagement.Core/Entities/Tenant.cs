using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyManagement.Core.Entities
{
    public class Tenant
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;
        
        [Phone]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string AlternatePhone { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string NationalId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string Occupation { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string Company { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MonthlyIncome { get; set; }
        
        [StringLength(200)]
        public string? EmergencyContact { get; set; }

        public DateTime? DateOfBirth { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}
