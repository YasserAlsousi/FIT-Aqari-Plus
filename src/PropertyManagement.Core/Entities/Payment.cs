using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyManagement.Core.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ReceiptNumber { get; set; } = string.Empty;

        [StringLength(50)]
        public string PaymentType { get; set; } = string.Empty;

        [StringLength(50)]
        public string Status { get; set; } = "Pending";
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime? PaymentDate { get; set; }

        [StringLength(50)]
        public string? PaymentMethod { get; set; }
        
        [StringLength(100)]
        public string? TransactionReference { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
        
        // Navigation Properties
        public int ContractId { get; set; }
        public Contract Contract { get; set; } = null!;
    }
}
