using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rental.API.Services;
using System.Security.Claims;

namespace Rental.API.Controllers
{
    [Route("api/transactions")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService transactionService;
        public TransactionController(ITransactionService transactionService)
        {
            this.transactionService = transactionService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUserTransactions()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await transactionService.GetUserTransactionsAsync(userId);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }
            return Ok(result.Data);
        }
    }
}
