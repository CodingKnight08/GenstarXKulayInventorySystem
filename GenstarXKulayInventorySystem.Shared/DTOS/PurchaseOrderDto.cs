using static GenstarXKulayInventorySystem.Shared.Helpers.OrdersHelper;

namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class PurchaseOrderDto:BaseEntityDto
{
    public int Id { get; set; }
    public int? SupplierId { get; set; }
    public SupplierDto? Supplier { get; set; }
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public PurchaseShipToOption PurchaseShipToOption { get; set; } = PurchaseShipToOption.GeneralSantosCity;
    public PurchaseRecieptOption PurchaseRecieptOption { get; set; } = PurchaseRecieptOption.NonBIR;
    public string? Remarks { get; set; }
    public DateTime PurchaseOrderDate { get; set; } = DateTime.Now;
    public DateTime? ExpectedDeliveryDate { get; set; }
    public bool IsRecieved { get; set; } = false;
    public PurchaseRecieveOption PurchaseRecieveOption { get; set; } = PurchaseRecieveOption.Pending;
    public decimal AssumeTotalAmount { get; set; } = 0;

    public List<PurchaseOrderItemDto> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItemDto>();
    public List<PurchaseOrderBillingDto> PurchaseOrderBillings { get; set; } = new List<PurchaseOrderBillingDto>();
    public void RecalculateAssumeTotalAmount()
    {
        AssumeTotalAmount = PurchaseOrderItems.Sum(item => (item.ItemAmount?? 0)* item.ItemQuantity);
    }
}
