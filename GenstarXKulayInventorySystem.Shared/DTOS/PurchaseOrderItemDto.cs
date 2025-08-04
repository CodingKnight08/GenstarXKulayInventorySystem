using System.ComponentModel.DataAnnotations.Schema;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class PurchaseOrderItemDto:BaseEntityDto
{
    public int Id { get; set; }
    public int? PurchaseOrderId { get; set; }
    public PurchaseOrderDto? PurchaseOrder { get; set; }

    public int? ProductId { get; set; }
    public ProductDto? Product { get; set; }

    public int? ProductBrandId { get; set; }
    public ProductBrandDto? ProductBrand { get; set; }
    public decimal ItemAmount { get; set; }
    public ProductMesurementOption? PurchaseItemMeasurementOption { get; set; }
    public string? ItemDescription { get; set; } = string.Empty;
    public int ItemQuantity { get; set; }
    public bool IsRecieved { get; set; } = false;
}
