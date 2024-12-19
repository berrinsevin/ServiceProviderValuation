using ServiceProviderRatingNuget.Domain.Entities;

namespace RatingService.Business.Services
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(int userId);
        Task AddUserAsync(UserDto user);
        Task DeleteUserAsync(int userId);
    }
}
