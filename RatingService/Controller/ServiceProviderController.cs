using Microsoft.AspNetCore.Mvc;
using RatingService.Business.Services;
using ServiceProviderRatingNuget.Domain.Entities;

namespace RatingService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProviderController : ControllerBase
    {
        private readonly IProviderService _providerService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderController"/> class.
        /// </summary>
        /// <param name="providerService">The provider service.</param>
        public ProviderController(IProviderService providerService)
        {
            _providerService = providerService;
        }

        /// <summary>
        /// Retrieves all providers.
        /// </summary>
        /// <returns>An action result containing the list of providers.</returns>
        [HttpGet]
        public async Task<IActionResult> GetProviders()
        {
            var providers = await _providerService.GetProvidersAsync();
            return Ok(providers);
        }

        /// <summary>
        /// Retrieves a provider by ID.
        /// </summary>
        /// <param name="id">The ID of the provider to retrieve.</param>
        /// <returns>An action result containing the provider details.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProviderById([FromQuery] int id)
        {
            var provider = await _providerService.GetProviderByIdAsync(id);
            if (provider == null)
            {
                return NotFound();
            }
            return Ok(provider);
        }

        /// <summary>
        /// Adds a new provider.
        /// </summary>
        /// <param name="provider">The provider to add.</param>
        /// <returns>An action result indicating the outcome of the operation.</returns>
        [HttpPost]
        public async Task<IActionResult> AddProvider([FromBody] ProviderDto provider)
        {
            try
            {
                await _providerService.AddProviderAsync(provider);
                return CreatedAtAction(nameof(GetProviderById), new { id = provider.Id }, provider);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Deletes a provider by ID.
        /// </summary>
        /// <param name="id">The ID of the provider to delete.</param>
        /// <returns>An action result indicating the outcome of the operation.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProvider(int id)
        {
            await _providerService.DeleteProviderAsync(id);
            return NoContent();
        }
    }
}
