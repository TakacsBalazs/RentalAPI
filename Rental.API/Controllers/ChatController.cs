using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rental.API.Models.Requests;
using Rental.API.Services;
using System.Security.Claims;

namespace Rental.API.Controllers
{
    [Route("api/conversations")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService chatService;

        public ChatController(IChatService chatService)
        {
            this.chatService = chatService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await chatService.CreateConversationAsync(request, userId);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }
            return Ok(result.Data);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUserConversations()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await chatService.GetUserConversationsAsync(userId);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }
            return Ok(result.Data);
        }

        [Authorize]
        [HttpPost("{id}/messages")]
        public async Task<IActionResult> CreateMessage([FromBody] CreateMessageRequest request, int id)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await chatService.CreateMessageAsync(request, id, userId);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }
            return Ok(result.Data);
        }

        [Authorize]
        [HttpGet("{id}/messages")]
        public async Task<IActionResult> GetConversationMessages(int id)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await chatService.GetConversationMessagesAsync(id, userId);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }
            return Ok(result.Data);
        }
    }
}
