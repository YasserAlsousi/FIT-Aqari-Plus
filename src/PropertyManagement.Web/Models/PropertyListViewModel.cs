using PropertyManagement.Core.Entities;

namespace PropertyManagement.Web.Models
{
    public class PropertyListViewModel
    {
        public List<Property> Properties { get; set; } = new();
        public PropertyFilterViewModel Filter { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    public class PropertyFilterViewModel
    {
        public string? PropertyType { get; set; }
        public string? Status { get; set; }
        public string? SearchTerm { get; set; }
        public decimal? MinRent { get; set; }
        public decimal? MaxRent { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
}