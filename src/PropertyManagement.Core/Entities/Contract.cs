using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyManagement.Core.Entities
{
    public class Contract
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ContractNumber { get; set; } = string.Empty;
        
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyRent { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SecurityDeposit { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(50)]
        public string Status { get; set; } = "Active";

        [StringLength(1000)]
        public string Terms { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
        
        // Navigation Properties
        public int PropertyId { get; set; }
        public Property Property { get; set; } = null!;
        
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;
        
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}