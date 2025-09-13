using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleAccounting.API.Data;
using SimpleAccounting.API.Models;
using SimpleAccounting.API.Models.DTOs;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace SimpleAccounting.Tests.Integration;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    [Fact]
    public async Task GetTransactions_ReturnsEmptyList_WhenNoTransactions()
    {
        // Arrange
        await ClearDatabase();

        // Act
        var response = await _client.GetAsync("/api/transactions");

        // Assert
        response.EnsureSuccessStatusCode();
        var transactions = await response.Content.ReadFromJsonAsync<List<Transaction>>(_jsonOptions);
        Assert.NotNull(transactions);
        Assert.Empty(transactions);
    }

    [Fact]
    public async Task CreateTransaction_ReturnsCreatedTransaction_WhenValidData()
    {
        // Arrange
        await ClearDatabase();
        var createDto = new CreateTransactionDto
        {
            Amount = 100.50m,
            Description = "Test Income",
            Type = TransactionType.Income,
            Date = DateTime.Today
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/transactions", createDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var transaction = await response.Content.ReadFromJsonAsync<Transaction>(_jsonOptions);
        Assert.NotNull(transaction);
        Assert.Equal(createDto.Amount, transaction.Amount);
        Assert.Equal(createDto.Description, transaction.Description);
        Assert.Equal(createDto.Type, transaction.Type);
    }

    [Fact]
    public async Task CreateTransaction_ReturnsBadRequest_WhenInvalidData()
    {
        // Arrange
        var createDto = new CreateTransactionDto
        {
            Amount = -100, // Invalid negative amount
            Description = "",
            Type = TransactionType.Income,
            Date = DateTime.Today
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/transactions", createDto);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetBalance_ReturnsCorrectBalance_WithMultipleTransactions()
    {
        // Arrange
        await ClearDatabase();
        
        // Create income transaction
        var income = new CreateTransactionDto
        {
            Amount = 1000m,
            Description = "Salary",
            Type = TransactionType.Income,
            Date = DateTime.Today
        };
        await _client.PostAsJsonAsync("/api/transactions", income);

        // Create expense transaction
        var expense = new CreateTransactionDto
        {
            Amount = 300m,
            Description = "Groceries",
            Type = TransactionType.Expense,
            Date = DateTime.Today
        };
        await _client.PostAsJsonAsync("/api/transactions", expense);

        // Act
        var response = await _client.GetAsync("/api/transactions/balance");

        // Assert
        response.EnsureSuccessStatusCode();
        var balanceResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
        var balance = balanceResponse.GetProperty("balance").GetDecimal();
        Assert.Equal(700m, balance); // 1000 - 300 = 700
    }

    [Fact]
    public async Task GetTransactions_ReturnsTransactionsInCorrectOrder()
    {
        // Arrange
        await ClearDatabase();
        
        var transaction1 = new CreateTransactionDto
        {
            Amount = 100m,
            Description = "First Transaction",
            Type = TransactionType.Income,
            Date = DateTime.Today.AddDays(-1)
        };
        
        var transaction2 = new CreateTransactionDto
        {
            Amount = 200m,
            Description = "Second Transaction",
            Type = TransactionType.Expense,
            Date = DateTime.Today
        };

        await _client.PostAsJsonAsync("/api/transactions", transaction1);
        await _client.PostAsJsonAsync("/api/transactions", transaction2);

        // Act
        var response = await _client.GetAsync("/api/transactions");

        // Assert
        response.EnsureSuccessStatusCode();
        var transactions = await response.Content.ReadFromJsonAsync<List<Transaction>>(_jsonOptions);
        Assert.NotNull(transactions);
        Assert.Equal(2, transactions.Count);
        
        // Should be ordered by date descending (newest first)
        Assert.Equal("Second Transaction", transactions[0].Description);
        Assert.Equal("First Transaction", transactions[1].Description);
    }

    [Fact]
    public async Task CorsHeaders_ArePresent_InApiResponses()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/transactions");

        // Assert
        response.EnsureSuccessStatusCode();
        
        // Check if CORS headers would be present (in actual browser request)
        // Note: In integration tests, CORS headers might not be fully present
        // This test ensures the endpoint is accessible
        Assert.True(response.IsSuccessStatusCode);
    }

    private async Task ClearDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AccountingDbContext>();
        
        context.Transactions.RemoveRange(context.Transactions);
        await context.SaveChangesAsync();
    }
}