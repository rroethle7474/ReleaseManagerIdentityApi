using ReleaseManagerIdentityApi.Models.DTOs;
using ReleaseManagerIdentityApi.Models.DTOs.Requests;

namespace ReleaseManagerIdentityApi.Services.UserService
{
    public interface IUserService
    {
        Task<UserProfileDto> GetUserProfileAsync(Guid userId);
        Task UpdateUserProfileAsync(Guid userId, UpdateProfileRequest request);
        Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    }
}