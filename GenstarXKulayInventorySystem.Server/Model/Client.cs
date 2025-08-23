namespace GenstarXKulayInventorySystem.Server.Model;

public class Client:BaseEntity
{
    public int Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string Address { get; set; }= string.Empty;
    public string ContactNumber = string.Empty;

    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}
