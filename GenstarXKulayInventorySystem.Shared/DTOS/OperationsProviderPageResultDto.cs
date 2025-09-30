namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class OperationsProviderPageResultDto<OperationsProviderDto>
{
    public int TotalCount { get; set; }
    public List<OperationsProviderDto> OperationsProviders { get; set; } = new List<OperationsProviderDto>();
}
