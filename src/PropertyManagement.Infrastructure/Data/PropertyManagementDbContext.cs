using Microsoft.EntityFrameworkCore;
using PropertyManagement.Core.Entities;

namespace PropertyManagement.Infrastructure.Data
{
    public class PropertyManagementDbContext : DbContext
    {
        public PropertyManagementDbContext(DbContextOptions<PropertyManagementDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Property> Properties { get; set; }
        public DbSet<Owner> Owners { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }
        public DbSet<PropertyImage> PropertyImages { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Property Configuration
            modelBuilder.Entity<Property>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MonthlyRent).HasPrecision(18, 2);
                entity.Property(e => e.SecurityDeposit).HasPrecision(18, 2);
                entity.Property(e => e.Area).HasPrecision(18, 2);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.PropertyType);

                entity.HasOne(e => e.Owner)
                    .WithMany(e => e.Properties)
                    .HasForeignKey(e => e.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Contract Configuration
            modelBuilder.Entity<Contract>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MonthlyRent).HasPrecision(18, 2);
                entity.Property(e => e.SecurityDeposit).HasPrecision(18, 2);
                entity.HasIndex(e => e.ContractNumber).IsUnique();
                
                entity.HasOne(e => e.Property)
                    .WithMany(e => e.Contracts)
                    .HasForeignKey(e => e.PropertyId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Tenant)
                    .WithMany(e => e.Contracts)
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Seed Data
            SeedData(modelBuilder);
        }
        
        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed sample data
            modelBuilder.Entity<Owner>().HasData(
                new Owner
                {
                    Id = 1,
                    FirstName = "أحمد",
                    LastName = "محمد",
                    Email = "ahmed.mohamed@example.com",
                    Phone = "+201234567890",
                    Address = "القاهرة، مصر",
                    NationalId = "12345678901234"
                }
            );
        }
    }
}