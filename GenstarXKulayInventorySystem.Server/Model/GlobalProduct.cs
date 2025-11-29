using System.ComponentModel.DataAnnotations;

namespace GenstarXKulayInventorySystem.Server.Model;

public class GlobalProduct:BaseEntity
{
    public int Id { get; set; }
    public int? BrandId { get; set; }
    public ProductBrand? ProductBrand { get; set; }
    [Required ] 
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Packaging { get; set; } = string.Empty;
    public ICollection<BranchProduct> BranchProducts { get; set; }  = new HashSet<BranchProduct>();
    public ICollection<RequestProductItem> RequestItems { get; set; } = new HashSet<RequestProductItem>();
}
