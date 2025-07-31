using System.ComponentModel.DataAnnotations.Schema;
using static GenstarXKulayInventorySystem.Shared.Helpers.OrdersHelper;

namespace GenstarXKulayInventorySystem.Server.Model;

public class PurchaseOrder:BaseEntity
{
    public int Id { get; set; }
    public int? SupplierId { get; set; }
    [ForeignKey(nameof(SupplierId))]
    public Supplier? Supplier { get; set; }
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public PurchaseShipToOption PurchaseShipToOption { get; set; } = PurchaseShipToOption.GeneralSantosCity;
    public PurchaseRecieptOption PurchaseRecieptOption { get; set; } = PurchaseRecieptOption.NonBIR;
    public string? Remarks { get; set; }
    public DateTime PurchaseOrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public bool IsRecieved { get; set; } = false;
    public PurchaseRecieveOption PurchaseRecieveOption { get; set; } = PurchaseRecieveOption.Pending;
    public decimal AssumeTotalAmount { get; set; } = 0;
}
