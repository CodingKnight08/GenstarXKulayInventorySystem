namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class ClientDto:BaseEntityDto
{
    public int Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ContactNumber = string.Empty;

    
}
