using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleAccounting.API.Data;
using SimpleAccounting.API.Models;
using System.Net;
using Xunit;

namespace SimpleAccounting.Tests.Integration;

public class PdfIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PdfIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetPdf_WithNoTransactions_ShouldReturnPdfFile()
    {
        // Act
        var response = await _client.GetAsync("/api/transactions/pdf");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        
        var content = await response.Content.ReadAsByteArrayAsync();
        Assert.True(content.Length > 0);
        
        // Check if it's a valid PDF (starts with PDF header)
        var pdfHeader = System.Text.Encoding.ASCII.GetString(content.Take(4).ToArray());
        Assert.Equal("%PDF", pdfHeader);
    }

    [Fact]
    public async Task GetPdf_WithTransactions_ShouldReturnPdfFile()
    {
        // Arrange - Add test data
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AccountingDbContext>();
        
        var transaction = new Transaction
        {
            Amount = 1000,
            Description = "Test Transaction",
            Type = TransactionType.Income,
            Date = DateTime.Now,
            CreatedAt = DateTime.Now
        };
        
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/transactions/pdf");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        
        var content = await response.Content.ReadAsByteArrayAsync();
        Assert.True(content.Length > 0);
        
        // Check if it's a valid PDF
        var pdfHeader = System.Text.Encoding.ASCII.GetString(content.Take(4).ToArray());
        Assert.Equal("%PDF", pdfHeader);
        
        // PDF with transactions should be larger than empty PDF
        Assert.True(content.Length > 5000); // Reasonable minimum for PDF with content
    }

    [Fact]
    public async Task GetPdf_ShouldHaveCorrectContentDisposition()
    {
        // Act
        var response = await _client.GetAsync("/api/transactions/pdf");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var contentDisposition = response.Content.Headers.ContentDisposition;
        Assert.NotNull(contentDisposition);
        Assert.Equal("attachment", contentDisposition.DispositionType);
        Assert.Contains("仕訳帳_", contentDisposition.FileName);
        Assert.Contains(".pdf", contentDisposition.FileName);
    }
}