using Rental.API.Common;
using Rental.API.Models.Responses;

namespace Rental.API.Services
{
    public interface ITransactionService
    {
        Task<Result<IEnumerable<TransactionResponse>>> GetUserTransactionsAsync(string userId);
    }
}
