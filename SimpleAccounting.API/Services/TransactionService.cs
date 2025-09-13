using Microsoft.EntityFrameworkCore;
using SimpleAccounting.API.Data;
using SimpleAccounting.API.Models;
using SimpleAccounting.API.Models.DTOs;

namespace SimpleAccounting.API.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly AccountingDbContext _context;

        public TransactionService(AccountingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
        {
            return await _context.Transactions
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Transaction> CreateTransactionAsync(CreateTransactionDto dto)
        {
            var transaction = new Transaction
            {
                Amount = dto.Amount,
                Description = dto.Description,
                Type = dto.Type,
                Date = dto.Date,
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return transaction;
        }

        public async Task<decimal> GetBalanceAsync()
        {
            var transactions = await _context.Transactions.ToListAsync();
            
            var income = transactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);
            
            var expenses = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            return income - expenses;
        }
    }
}