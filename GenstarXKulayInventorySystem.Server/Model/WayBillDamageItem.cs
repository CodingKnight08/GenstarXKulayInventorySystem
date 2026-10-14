namespace GenstarXKulayInventorySystem.Server.Model;

public class WayBillDamageItem:BaseEntity
{
    public int Id { get; set; }
    public int? WayBillItemId { get; set; }
    public WayBillItems? WayBillItem { get; set; }
    public decimal DamageQuantity { get; set; } = 0;
    public decimal DamageAmount { get; set;} = 0;
    public decimal TotalDamageCost { get; set; } = 0;
}
