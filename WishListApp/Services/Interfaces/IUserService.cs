using WishListApp.DTOs;

namespace WishListApp.Interfaces;

public interface IUserService
{
    Task<UserDtos.UserResponse?> GetByIdAsync(Guid userId);
    Task<List<UserDtos.UserResponse>> SearchByEmailAsync(string email);
    Task<UserDtos.UserResponse?> UpdateProfileAsync(Guid userId, UserDtos.UpdateUserRequest request);
}