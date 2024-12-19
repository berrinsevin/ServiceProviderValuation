using Serilog;
using ServiceProviderRatingNuget.Domain.Entities;
using ServiceProviderRatingNuget.DataAccess.Repositories;

namespace RatingService.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="userRepository">The provider service.</param>
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary> 
        /// Retrieves a user by ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>The user with the specified ID.</returns>
        /// <exception cref="ArgumentException">Thrown when the userId is less than or equal to zero.</exception>
        /// <exception cref="Exception">Thrown when an error occurs while retrieving the user.</exception>
        public async Task<User> GetUserByIdAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User ID must be greater than zero.", nameof(userId));
            }

            try
            {
                return await _userRepository.GetUserByIdAsync(userId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving user with ID {UserId}", userId);
                throw;
            }
        }

        /// <summary> 
        /// Adds a user
        /// </summary>
        /// <param name="user">The user object containing user details</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <exception cref="ArgumentException">Thrown when the user name is empty.</exception>
        /// <exception cref="Exception">Thrown when an error occurs while adding the user.</exception>
        public async Task AddUserAsync(UserDto user)
        {
            if (string.IsNullOrEmpty(user.Name))
            {
                throw new ArgumentException("User name cannot be empty.", nameof(user.Name));
            }

            try
            {
                await _userRepository.AddUserAsync(new() {
                    Name = user.Name
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while adding user {UserName}", user.Name);
                throw;
            }
        }

        /// <summary>
        /// Deletes a user by ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown when the userId is less than or equal to zero.</exception> 
        /// <exception cref="Exception">Thrown when an error occurs while deleting the user.</exception>
        public async Task DeleteUserAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User ID must be greater than zero.", nameof(userId));
            }

            try
            {
                await _userRepository.DeleteUserAsync(userId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting user with ID {UserId}", userId);
                throw;
            }
        }
    }
}
