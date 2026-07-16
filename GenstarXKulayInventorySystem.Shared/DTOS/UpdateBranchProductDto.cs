namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class UpdateBranchProductDto
{
  public int  BranchProductId { get; set; }
  public decimal  BufferStock { get; set; }
  public decimal  ActualQuantity { get; set; }  
}
