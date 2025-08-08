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
        _ = CreateMap<PurchaseOrder, PurchaseOrderDto>().ReverseMap();
        _ = CreateMap<PurchaseOrderItem, PurchaseOrderItemDto>()
                .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product))
                .ForMember(dest => dest.ProductBrand, opt => opt.MapFrom(src => src.ProductBrand))
                .ForMember(dest => dest.PurchaseOrder, opt => opt.Ignore()).ReverseMap();
        _ = CreateMap<Billing,BillingDto>().ReverseMap();

    }
}
