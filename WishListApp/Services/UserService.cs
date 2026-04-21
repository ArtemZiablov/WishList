using WishListApp.DTOs;
using WishListApp.Interfaces;

namespace WishListApp.Services;

public class UserService: IUserService
{
    public async Task<UserDtos.UserResponse?> GetByIdAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public async Task<List<UserDtos.UserResponse>> SearchByEmailAsync(string email)
    {
        throw new NotImplementedException();
    }

    public async Task<UserDtos.UserResponse?> UpdateProfileAsync(Guid userId, UserDtos.UpdateUserRequest request)
    {
        throw new NotImplementedException();
    }
}