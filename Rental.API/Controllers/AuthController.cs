using Microsoft.AspNetCore.Mvc;
using Rental.API.Models.Requests;

namespace Rental.API.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;

        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUserRequest request)
        {
            var result = await authService.RegisterAsync(request);
            if(!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }
            return Ok("Successfull registration!");
        }
    }
}
