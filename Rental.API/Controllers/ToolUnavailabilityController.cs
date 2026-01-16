using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rental.API.Models.Requests;
using Rental.API.Services;
using System.Security.Claims;

namespace Rental.API.Controllers
{
    [Route("api/toolunavailability")]
    [ApiController]
    public class ToolUnavailabilityController : ControllerBase
    {
        private readonly IToolUnavailabilityService toolUnavailabilityService;

        public ToolUnavailabilityController(IToolUnavailabilityService toolUnavailabilityService)
        {
            this.toolUnavailabilityService = toolUnavailabilityService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateToolUnavailability(CreateToolUnavailabilityRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await toolUnavailabilityService.CreateToolUnavailabilityAsync(request, userId);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }

            return Ok(result.Data);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteToolUnavailability(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await toolUnavailabilityService.DeleteToolUnavailabilityAsync(id, userId);

            if(!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }
            return Ok("Successful delete!");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllByToolId([FromQuery] GetToolUnavailabilitiesRequest request)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await toolUnavailabilityService.GetToolUnavailabilitiesAsync(request, userId);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }

            return Ok(result.Data);
        }
    }
}
