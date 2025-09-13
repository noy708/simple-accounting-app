using SimpleAccounting.API.Models;
using SimpleAccounting.API.Models.DTOs;

namespace SimpleAccounting.API.Services
{
    public interface ITransactionService
    {
        Task<IEnumerable<Transaction>> GetAllTransactionsAsync();
        Task<Transaction> CreateTransactionAsync(CreateTransactionDto dto);
        Task<decimal> GetBalanceAsync();
    }
}