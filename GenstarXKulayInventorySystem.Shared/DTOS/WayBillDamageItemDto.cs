namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class WayBillDamageItemDto : BaseEntityDto
{
    public int Id { get; set; }
    public int? WayBillItemId { get; set; }
    public WayBillItemsDto? WayBillItem { get; set; }
    public decimal DamageQuantity { get; set; } = 0;
    public decimal DamageAmount { get; set; } = 0;
    public decimal TotalDamageCost { get; set; } = 0;
}
