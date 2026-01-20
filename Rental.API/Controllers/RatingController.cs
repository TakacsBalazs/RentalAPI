using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rental.API.Models.Requests;
using Rental.API.Services;
using System.Security.Claims;

namespace Rental.API.Controllers
{
    [Route("api/ratings")]
    [ApiController]
    public class RatingController : ControllerBase
    {
        private readonly IRatingService ratingService;
        public RatingController(IRatingService ratingService)
        {
            this.ratingService = ratingService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateRating([FromBody] CreateRatingRequest request)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await ratingService.CreateRatingAsync(request, userId);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }
            return Ok(result.Data);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUserRatingsAsync([FromQuery] GetRatingsRequest request)
        {
            var result = await ratingService.GetUserRatingsAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }
            return Ok(result.Data);
        }

        [Authorize]
        [HttpDelete("{ratedUserId}")]
        public async Task<IActionResult> DeleteRating(string ratedUserId)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await ratingService.DeleteRatingAsync(ratedUserId, userId);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }
            return Ok("Successful delete this rating!");
        }
    }
}
