using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Model;

public class PurchaseOrderItem:BaseEntity
{
    public int Id { get; set; }
    
    public int? PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; } 

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    public int? ProductBrandId { get; set; }
    public ProductBrand? ProductBrand { get; set; }

    public decimal ItemAmount { get; set; }
    public ProductMesurementOption? PurchaseItemMeasurementOption { get; set; }
    public int ItemQuantity { get; set; }
    public string? ItemDescription { get; set; } = string.Empty;
    public bool IsRecieved { get; set; } = false;


}
