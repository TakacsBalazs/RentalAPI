using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rental.API.Services;
using System.Security.Claims;

namespace Rental.API.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService notificationSercvice;
        public NotificationController(INotificationService notificationSercvice)
        {
            this.notificationSercvice = notificationSercvice;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUserNotifications()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await notificationSercvice.GetUserNotificationsAsync(userId);
            if(!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }
            return Ok(result.Data);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotificationById(int id)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await notificationSercvice.GetNotificationByIdAsync(id, userId);
            if(!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }
            return Ok(result.Data);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await notificationSercvice.DeleteNotificationAsync(id, userId);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }
            return Ok("Successful delete!");
        }
    }
}
