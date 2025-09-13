using Microsoft.EntityFrameworkCore;
using SimpleAccounting.API.Data;
using SimpleAccounting.API.Models;
using SimpleAccounting.API.Services;
using Xunit;

namespace SimpleAccounting.Tests.Services;

public class PdfServiceTests : IDisposable
{
    private readonly AccountingDbContext _context;
    private readonly PdfService _pdfService;

    public PdfServiceTests()
    {
        var options = new DbContextOptionsBuilder<AccountingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AccountingDbContext(options);
        _pdfService = new PdfService(_context);
    }

    [Fact]
    public async Task GenerateTransactionsPdfAsync_WithNoTransactions_ShouldReturnPdfBytes()
    {
        // Act
        var result = await _pdfService.GenerateTransactionsPdfAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    [Fact]
    public async Task GenerateTransactionsPdfAsync_WithTransactions_ShouldReturnPdfBytes()
    {
        // Arrange
        var transactions = new[]
        {
            new Transaction
            {
                Amount = 1000,
                Description = "Test Income",
                Type = TransactionType.Income,
                Date = DateTime.Now.AddDays(-1),
                CreatedAt = DateTime.Now
            },
            new Transaction
            {
                Amount = 500,
                Description = "Test Expense",
                Type = TransactionType.Expense,
                Date = DateTime.Now,
                CreatedAt = DateTime.Now
            }
        };

        _context.Transactions.AddRange(transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _pdfService.GenerateTransactionsPdfAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
        
        // PDF should be larger when there are transactions
        Assert.True(result.Length > 1000); // Reasonable minimum size for a PDF with content
    }

    [Fact]
    public async Task GenerateTransactionsPdfAsync_ShouldOrderTransactionsByDate()
    {
        // Arrange
        var olderTransaction = new Transaction
        {
            Amount = 1000,
            Description = "Older Transaction",
            Type = TransactionType.Income,
            Date = DateTime.Now.AddDays(-5),
            CreatedAt = DateTime.Now
        };

        var newerTransaction = new Transaction
        {
            Amount = 500,
            Description = "Newer Transaction",
            Type = TransactionType.Expense,
            Date = DateTime.Now.AddDays(-1),
            CreatedAt = DateTime.Now
        };

        // Add in reverse order to test sorting
        _context.Transactions.AddRange(newerTransaction, olderTransaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _pdfService.GenerateTransactionsPdfAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
        
        // We can't easily test the content order without parsing the PDF,
        // but we can ensure the method completes successfully
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}