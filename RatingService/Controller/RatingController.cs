using Serilog;
using Microsoft.AspNetCore.Mvc;
using RatingService.Business.Services;
using ServiceProviderRatingNuget.Domain.Entities;

namespace RatingService.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class RatingController : ControllerBase
    {
        private readonly IRateService _ratingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RatingController"/> class.
        /// </summary>
        /// <param name="ratingService">The rating service.</param>
        public RatingController(IRateService ratingService)
        {
            _ratingService = ratingService;
        }

        /// <summary>
        /// Adds a new rating
        /// </summary>
        /// <param name="rating">The rating to add.</param>
        /// <returns>An action result indicating the outcome of the operation.</returns>
        [HttpPost]
        public async Task<IActionResult> AddRating([FromBody] RatingDto rating)
        {
            try
            {
                await _ratingService.AddRatingAsync(rating);
                return Ok();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error submitting rating");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
