namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class SourceAndRequesteeProductDto
{
   public int? MasterProductId { get; set; }
   public GlobalProductDto? MasterProduct { get; set; }
   public BranchProductDto? SourceProduct { get; set; }
   public BranchProductDto? RequesterProduct { get; set; }
}
