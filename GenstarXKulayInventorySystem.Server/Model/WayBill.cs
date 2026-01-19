namespace GenstarXKulayInventorySystem.Server.Model;

public class WayBill:BaseEntity
{
    public int Id { get; set; }
    public string WayBillNumber { get; set; } = string.Empty;
    public string Courier { get;set;  } = string.Empty;
    public int SupplierId { get; set; } 
    public Supplier? Supplier { get; set; }
    public DateTime DateReceived { get; set; }
    public ICollection<WayBillItems> WayBillItems { get; set; } = new List<WayBillItems>();
}



