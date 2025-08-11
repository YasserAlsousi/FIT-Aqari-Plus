using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyManagement.Core.Entities
{
    public class Property
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string PropertyType { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [StringLength(100)]
        public string City { get; set; } = string.Empty;
        
        public decimal Area { get; set; }
        
        public int Bedrooms { get; set; }
        
        public int Bathrooms { get; set; }
        
        public int? Floor { get; set; }

        public bool HasParking { get; set; }
        public bool HasElevator { get; set; }
        public bool HasBalcony { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyRent { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SecurityDeposit { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Available";
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
        
        // Navigation Properties
        public int OwnerId { get; set; }
        public Owner Owner { get; set; } = null!;
        
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}