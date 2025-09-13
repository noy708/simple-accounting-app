using Microsoft.EntityFrameworkCore;
using SimpleAccounting.API.Data;
using SimpleAccounting.API.Models;
using SimpleAccounting.API.Models.DTOs;
using SimpleAccounting.API.Services;
using Xunit;

namespace SimpleAccounting.Tests.Services
{
    public class TransactionServiceTests : IDisposable
    {
        private readonly AccountingDbContext _context;
        private readonly TransactionService _service;

        public TransactionServiceTests()
        {
            var options = new DbContextOptionsBuilder<AccountingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AccountingDbContext(options);
            _service = new TransactionService(_context);
        }

        [Fact]
        public async Task GetAllTransactionsAsync_ReturnsEmptyList_WhenNoTransactions()
        {
            // Act
            var result = await _service.GetAllTransactionsAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllTransactionsAsync_ReturnsTransactionsOrderedByDateDesc()
        {
            // Arrange
            var transaction1 = new Transaction
            {
                Amount = 100,
                Description = "First",
                Type = TransactionType.Income,
                Date = DateTime.Today.AddDays(-2),
                CreatedAt = DateTime.UtcNow.AddMinutes(-10)
            };

            var transaction2 = new Transaction
            {
                Amount = 50,
                Description = "Second",
                Type = TransactionType.Expense,
                Date = DateTime.Today.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddMinutes(-5)
            };

            _context.Transactions.AddRange(transaction1, transaction2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllTransactionsAsync();
            var transactions = result.ToList();

            // Assert
            Assert.Equal(2, transactions.Count);
            Assert.Equal("Second", transactions[0].Description); // Most recent date first
            Assert.Equal("First", transactions[1].Description);
        }

        [Fact]
        public async Task CreateTransactionAsync_CreatesTransaction_WithValidDto()
        {
            // Arrange
            var dto = new CreateTransactionDto
            {
                Amount = 150.50m,
                Description = "Test transaction",
                Type = TransactionType.Income,
                Date = DateTime.Today
            };

            // Act
            var result = await _service.CreateTransactionAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal(dto.Amount, result.Amount);
            Assert.Equal(dto.Description, result.Description);
            Assert.Equal(dto.Type, result.Type);
            Assert.Equal(dto.Date, result.Date);
            Assert.True(result.CreatedAt <= DateTime.UtcNow);

            // Verify it was saved to database
            var savedTransaction = await _context.Transactions.FindAsync(result.Id);
            Assert.NotNull(savedTransaction);
            Assert.Equal(dto.Amount, savedTransaction.Amount);
        }

        [Fact]
        public async Task GetBalanceAsync_ReturnsZero_WhenNoTransactions()
        {
            // Act
            var balance = await _service.GetBalanceAsync();

            // Assert
            Assert.Equal(0, balance);
        }

        [Fact]
        public async Task GetBalanceAsync_CalculatesCorrectBalance_WithIncomeAndExpenses()
        {
            // Arrange
            var income1 = new Transaction
            {
                Amount = 1000,
                Description = "Salary",
                Type = TransactionType.Income,
                Date = DateTime.Today,
                CreatedAt = DateTime.UtcNow
            };

            var income2 = new Transaction
            {
                Amount = 500,
                Description = "Bonus",
                Type = TransactionType.Income,
                Date = DateTime.Today,
                CreatedAt = DateTime.UtcNow
            };

            var expense1 = new Transaction
            {
                Amount = 300,
                Description = "Rent",
                Type = TransactionType.Expense,
                Date = DateTime.Today,
                CreatedAt = DateTime.UtcNow
            };

            var expense2 = new Transaction
            {
                Amount = 100,
                Description = "Groceries",
                Type = TransactionType.Expense,
                Date = DateTime.Today,
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.AddRange(income1, income2, expense1, expense2);
            await _context.SaveChangesAsync();

            // Act
            var balance = await _service.GetBalanceAsync();

            // Assert
            // Income: 1000 + 500 = 1500
            // Expenses: 300 + 100 = 400
            // Balance: 1500 - 400 = 1100
            Assert.Equal(1100, balance);
        }

        [Fact]
        public async Task GetBalanceAsync_ReturnsNegativeBalance_WhenExpensesExceedIncome()
        {
            // Arrange
            var income = new Transaction
            {
                Amount = 100,
                Description = "Small income",
                Type = TransactionType.Income,
                Date = DateTime.Today,
                CreatedAt = DateTime.UtcNow
            };

            var expense = new Transaction
            {
                Amount = 200,
                Description = "Large expense",
                Type = TransactionType.Expense,
                Date = DateTime.Today,
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.AddRange(income, expense);
            await _context.SaveChangesAsync();

            // Act
            var balance = await _service.GetBalanceAsync();

            // Assert
            Assert.Equal(-100, balance);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}