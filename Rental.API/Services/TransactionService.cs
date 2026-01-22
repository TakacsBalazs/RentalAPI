using Microsoft.EntityFrameworkCore;
using Rental.API.Common;
using Rental.API.Data;
using Rental.API.Models.Responses;

namespace Rental.API.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly AppDbContext context;
        public TransactionService(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<Result<IEnumerable<TransactionResponse>>> GetUserTransactionsAsync(string userId)
        {
            bool isUserExist = await context.Users.AnyAsync(x => x.Id == userId);
            if (!isUserExist)
            {
                return Result<IEnumerable<TransactionResponse>>.Failure("Invalid User Id!");
            }
            var response = await context.Transactions.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAt).Select(x => new TransactionResponse
            {
                Id = x.Id,
                UserId = x.UserId,
                Amount = x.Amount,
                CreatedAt = x.CreatedAt,
                Type = x.Type,
                Description = x.Description,
                BalanceSnapshot = x.BalanceSnapshot,
                LockedBalanceSnapshot = x.LockedBalanceSnapshot,
                BookingId = x.BookingId
            }).ToListAsync();

            return Result<IEnumerable<TransactionResponse>>.Success(response);
        }
    }
}
