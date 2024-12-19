using Microsoft.AspNetCore.Mvc;
using RatingService.Business.Services;
using ServiceProviderRatingNuget.Domain.Entities;

namespace RatingService.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class.
        /// </summary>
        /// <param name="userService">The user service.</param>
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary> 
        /// Retrieves a user by ID
        /// </summary>
        /// <param name="id">The ID of the user to retrieve.</param> 
        /// <returns>An action result containing the user details.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById([FromQuery] int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        /// <summary>
        /// Adds a new user
        /// </summary>
        /// <param name="user">The user to add.</param>
        /// <returns>An action result indicating the outcome of the operation.</returns>
        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] UserDto user)
        {
            await _userService.AddUserAsync(user);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }

        /// <summary>
        /// Deletes a user by ID
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param> 
        /// <returns>An action result indicating the outcome of the operation.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromQuery] int id)
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }
    }
}