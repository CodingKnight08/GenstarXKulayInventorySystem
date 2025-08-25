using AutoMapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace GenstarXKulayInventorySystem.Server.Mapper;

public class AutoMapperProfile: Profile
{

    readonly JsonSerializerOptions options = new JsonSerializerOptions
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        WriteIndented = true,
    };
    public AutoMapperProfile()
    {
        _ =CreateMap<User, UserDto>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ReverseMap()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());
        _ = CreateMap<BaseEntity, BaseEntityDto>()
            .ReverseMap();

        _ = CreateMap<ProductBrand, ProductBrandDto>().ReverseMap();
        _ = CreateMap<ProductCategory, ProductCategoryDto>().ReverseMap();
        _ = CreateMap<Product, ProductDto>().ReverseMap();
        _ = CreateMap<Supplier,SupplierDto>().ReverseMap();
        _ = CreateMap<PurchaseOrder, PurchaseOrderDto>()
                .ForMember(dest => dest.PurchaseOrderBillings, opt => opt.Ignore()) 
                .ReverseMap()
                .ForMember(dest => dest.PurchaseOrderBillings, opt => opt.Ignore());

        _ = CreateMap<PurchaseOrderItem, PurchaseOrderItemDto>()
                .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product))
                .ForMember(dest => dest.ProductBrand, opt => opt.MapFrom(src => src.ProductBrand))
                .ForMember(dest => dest.PurchaseOrder, opt => opt.Ignore()).ReverseMap();
        _ = CreateMap<Billing,BillingDto>().ReverseMap();
        _ = CreateMap<PurchaseOrderBilling, PurchaseOrderBillingDto>()
            .ForMember(dest => dest.PurchaseOrder, opt => opt.MapFrom(src => src.PurchaseOrder))
            .ReverseMap()
            .ForMember(dest => dest.PurchaseOrder, opt => opt.Ignore());
        _ = CreateMap<DailySale, DailySaleDto>().ReverseMap();
        _ = CreateMap<SaleItem, SaleItemDto>()
               .ForMember(dest => dest.DataList, opt => opt.MapFrom(src => DeserializeInvolvePaints(src.DataList)))
               .ReverseMap()
               .ForMember(dest => dest.DataList, opt => opt.MapFrom(src => SerializeInvolvePaints(src.DataList)));
        _ = CreateMap<Client, ClientDto>().ReverseMap();
    }

    private List<InvolvePaintsDto>? DeserializeInvolvePaints(string datalistJson)
    {
        List<InvolvePaintsDto> productData = JsonSerializer.Deserialize<List<InvolvePaintsDto>>(datalistJson, options)
           ?? new List<InvolvePaintsDto>();
        return productData;
    }

    private string SerializeInvolvePaints(List<InvolvePaintsDto> productData) 
    {
        return JsonSerializer.Serialize(productData, options);
    }
}
