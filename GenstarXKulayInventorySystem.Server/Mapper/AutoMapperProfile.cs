using AutoMapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace GenstarXKulayInventorySystem.Server.Mapper;

public class AutoMapperProfile : Profile
{

    readonly JsonSerializerOptions options = new JsonSerializerOptions
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        WriteIndented = true,
    };
    public AutoMapperProfile()
    {
        _ = CreateMap<User, UserDto>().ReverseMap();

        _ = CreateMap<BaseEntity, BaseEntityDto>()
            .ReverseMap();

        _ = CreateMap<ProductBrand, ProductBrandDto>().ReverseMap();
        _ = CreateMap<ProductCategory, ProductCategoryDto>().ReverseMap();
        _ = CreateMap<Product, ProductDto>().ReverseMap();
        _ = CreateMap<Supplier, SupplierDto>().ReverseMap();
        _ = CreateMap<PurchaseOrder, PurchaseOrderDto>()
                .ForMember(dest => dest.PurchaseOrderBillings, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.PurchaseOrderBillings, opt => opt.Ignore());

        _ = CreateMap<PurchaseOrderItem, PurchaseOrderItemDto>()
                .ForMember(dest => dest.BranchProduct, opt => opt.MapFrom(src => src.BranchProduct))
                .ForMember(dest => dest.ProductBrand, opt => opt.MapFrom(src => src.ProductBrand))
                .ForMember(dest => dest.PurchaseOrder, opt => opt.Ignore()).ReverseMap();
        _ = CreateMap<Billing, BillingDto>()
                    .ForMember(dest => dest.OperationsProvider, opt => opt.MapFrom(src => src.OperationsProvider))
                    .ReverseMap();

        _ = CreateMap<PurchaseOrderBilling, PurchaseOrderBillingDto>()
            .ForMember(dest => dest.PurchaseOrder, opt => opt.MapFrom(src => src.PurchaseOrder))
            .ReverseMap()
            .ForMember(dest => dest.PurchaseOrder, opt => opt.Ignore());
        _ = CreateMap<DailySale, DailySaleDto>()
              .ForMember(dest => dest.Client, opt => opt.Ignore())
              .ForMember(dest => dest.SaleItemsCount, opt => opt.MapFrom(src => src.SaleItems.Count))
              .ReverseMap();

        _ = CreateMap<SaleItem, SaleItemDto>()
               .ForMember(dest => dest.DataList, opt => opt.MapFrom(src => DeserializeInvolvePaints(src.DataList)))
               .ReverseMap()
               .ForMember(dest => dest.DataList, opt => opt.MapFrom(src => SerializeInvolvePaints(src.DataList)));
        _ = CreateMap<Model.Client, ClientDto>()
                 .ForMember(dest => dest.DailySales, opt => opt.MapFrom(src => src.DailySales))
                 .ReverseMap();

        _ = CreateMap<DailySaleReport, DailySaleReportDto>().ReverseMap();
        _ = CreateMap<Registration, RegistrationDto>().ReverseMap();
        _ = CreateMap<OperationsProvider, OperationsProviderDto>()
            .ForMember(dest => dest.Billings, opt => opt.Ignore()).ReverseMap();

        _ = CreateMap<RequestProductItem, RequestProductItemDto>()
                .ForMember(d => d.MasterProduct,
                    o => o.MapFrom(s => s.MasterProduct))
                .ForMember(d => d.ProductSourceBranch,
                    o => o.MapFrom(s => s.ProductSourceBranch))
                .ForMember(d => d.ProductRequesterBranch,
                    o => o.MapFrom(s => s.ProductRequesterBranch))
                .ForMember(d => d.Branch,
                    o => o.MapFrom(s => s.Branch))
                .ForMember(d => d.SourceProduct,
                    o => o.MapFrom(s => s.SourceProduct));
        _ = CreateMap<RequestProductItemDto, RequestProductItem>()
            .ForMember(e => e.MasterProduct, o => o.Ignore())
            .ForMember(e => e.ProductSourceBranch, o => o.Ignore())
            .ForMember(e => e.ProductRequesterBranch, o => o.Ignore())
            .ForMember(e => e.PullOutRequest, o => o.Ignore());

        _ = CreateMap<PullOutRequest, PullOutRequestDto>().ReverseMap();
        _ = CreateMap<GlobalProduct, GlobalProductDto>().ReverseMap();
        _ = CreateMap<BranchProduct, BranchProductDto>()
             .ForMember(d => d.MasterProduct, o => o.Ignore())
             .ForMember(d => d.TiedUpProduct, o => o.Ignore())
             .ForMember(d => d.BasisProduct, o => o.Ignore());

        _ =  CreateMap<BranchProductDto, BranchProduct>()
                .ForMember(d => d.MasterProduct, o => o.Ignore())
                .ForMember(d => d.TiedUpProduct, o => o.Ignore())
                .ForMember(d => d.BasisProduct, o => o.Ignore())
                .ForMember(d => d.SaleItems, o => o.Ignore());
        _ = CreateMap<ReturnItem, ReturnItemDto>().ReverseMap();

      
        _ =CreateMap<WayBill, WayBillDto>()
            .ForMember(dest => dest.WayBillItems, opt => opt.MapFrom(src => src.WayBillItems))
            .ForMember(dest => dest.Supplier, opt => opt.MapFrom(src => src.Supplier));

        _ = CreateMap<WayBillDto, WayBill>()
            .ForMember(dest => dest.Supplier, opt => opt.Ignore()) // avoid circular
            .ForMember(dest => dest.WayBillItems, opt => opt.MapFrom(src => src.WayBillItems));


        // WayBillItems ↔ WayBillItemsDto
        _ = CreateMap<WayBillItems, WayBillItemsDto>()
            .ForMember(dest => dest.BranchProduct, opt => opt.MapFrom(src => src.BranchProduct))
            .ForMember(dest => dest.WayBill, opt => opt.Ignore()) // prevent circular reference
            .ReverseMap()
            .ForMember(dest => dest.BranchProduct, opt => opt.Ignore())
            .ForMember(dest => dest.WayBill, opt => opt.Ignore());

        _ = CreateMap<WayBillDamageItem, WayBillDamageItemDto>()
            .ForMember(dest => dest.WayBillItem, opt => opt.MapFrom(src => src.WayBillItem));

        _ = CreateMap<WayBillDamageItemDto, WayBillDamageItem>()
            .ForMember(dest => dest.WayBillItem, opt => opt.Ignore());

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
