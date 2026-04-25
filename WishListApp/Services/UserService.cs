using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WishListApp.DTOs;
using WishListApp.Interfaces;

namespace WishListApp.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public UserService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<UserDtos.UserResponse?> GetByIdAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);
        return user is null ? null : _mapper.Map<UserDtos.UserResponse?>(user);
    }

    public async Task<List<UserDtos.UserResponse>> SearchByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return new List<UserDtos.UserResponse>();

        // Case-insensitive partial match
        var users = await _db.Users
            .Where(u => u.Email != null && u.Email.ToLower().Contains(email.ToLower()))
            .Take(10)
            .ToListAsync();

        return _mapper.Map<List<UserDtos.UserResponse>>(users);
    }

    public async Task<UserDtos.UserResponse?> UpdateProfileAsync(Guid userId, UserDtos.UpdateUserRequest request)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return null;

        _mapper.Map(request, user);
        await _db.SaveChangesAsync();

        return _mapper.Map<UserDtos.UserResponse>(user);
    }
}