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

    }
}
