using AutoMapper;
using WishListApp.Models;
using WishListApp.DTOs;

namespace WishListApp.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // WishList
        CreateMap<WishList, WishListDtos.WishListResponse>()
            .ForMember(dest => dest.ItemCount, 
                opt => 
                    opt.MapFrom(src => src.Items.Count));
        CreateMap<WishListDtos.CreateWishListRequest, WishList>();
        CreateMap<WishListDtos.UpdateWishListRequest, WishList>();

        // WishListItem
        CreateMap<WishListItem, WishListItemDtos.WishListItemResponse>()
            .ForMember(dest => dest.IsBooked, 
                opt => 
                    opt.MapFrom(src => src.Booking != null));
        CreateMap<WishListItemDtos.CreateWishListItemRequest, WishListItem>();
        CreateMap<WishListItemDtos.UpdateWishListItemRequest, WishListItem>();
        
        // User
        CreateMap<User, UserDtos.UserResponse>();
        CreateMap<UserDtos.UpdateUserRequest, User>();

        // Friendship
        CreateMap<Friendship, FriendshipDtos.FriendshipResponse>()
            .ForMember(dest => dest.RequesterName,
                opt => opt.MapFrom(src => src.Requester.DisplayName))
            .ForMember(dest => dest.RequesterAvatarUrl,
                opt => opt.MapFrom(src => src.Requester.AvatarUrl));
    }
}