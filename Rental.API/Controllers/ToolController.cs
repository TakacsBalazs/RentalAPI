using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rental.API.Models.Requests;
using Rental.API.Services;
using System.Security.Claims;

namespace Rental.API.Controllers
{
    [Route("api/tool")]
    [ApiController]
    public class ToolController : ControllerBase
    {
        private readonly IToolService toolService;

        public ToolController(IToolService toolService)
        {
            this.toolService = toolService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateTool(CreateToolRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await toolService.CreateToolAsync(request, userId);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }

            return Ok(result.Data);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllTool([FromQuery] GetToolsRequest request)
        {
            var result = await toolService.GetAllToolAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }
            return Ok(result.Data);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetToolById(int id)
        {
            var result = await toolService.GetToolByIdAsync(id);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }
            return Ok(result.Data);
        }
    }
}
