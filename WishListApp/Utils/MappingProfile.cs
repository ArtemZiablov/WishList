using AutoMapper;
using WishListApp.Models;
using WishListApp.DTOs;

namespace WishListApp.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<WishList, WishListDtos.WishListResponse>()
            .ForMember(dest => dest.ItemCount, opt => 
                opt.MapFrom(src => src.Items.Count));
        
        CreateMap<WishListDtos.CreateWishListRequest, WishList>();
        
        CreateMap<WishListDtos.UpdateWishListRequest, WishList>();
    }
}