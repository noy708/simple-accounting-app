using Microsoft.AspNetCore.Mvc;
using Moq;
using SimpleAccounting.API.Controllers;
using SimpleAccounting.API.Models;
using SimpleAccounting.API.Models.DTOs;
using SimpleAccounting.API.Services;
using Xunit;

namespace SimpleAccounting.Tests.Controllers
{
    public class TransactionsControllerTests
    {
        private readonly Mock<ITransactionService> _mockTransactionService;
        private readonly Mock<IPdfService> _mockPdfService;
        private readonly TransactionsController _controller;

        public TransactionsControllerTests()
        {
            _mockTransactionService = new Mock<ITransactionService>();
            _mockPdfService = new Mock<IPdfService>();
            _controller = new TransactionsController(_mockTransactionService.Object, _mockPdfService.Object);
        }

        [Fact]
        public async Task GetTransactions_ReturnsOkResult_WithTransactionList()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                new Transaction
                {
                    Id = 1,
                    Amount = 100,
                    Description = "Test",
                    Type = TransactionType.Income,
                    Date = DateTime.Today,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockTransactionService.Setup(s => s.GetAllTransactionsAsync())
                       .ReturnsAsync(transactions);

            // Act
            var result = await _controller.GetTransactions();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTransactions = Assert.IsAssignableFrom<IEnumerable<TransactionResponseDto>>(okResult.Value);
            var transactionList = returnedTransactions.ToList();
            
            Assert.Single(transactionList);
            Assert.Equal(1, transactionList[0].Id);
            Assert.Equal(100, transactionList[0].Amount);
            Assert.Equal("Test", transactionList[0].Description);
            Assert.Equal("Income", transactionList[0].Type);
        }

        [Fact]
        public async Task GetTransactions_ReturnsInternalServerError_WhenServiceThrows()
        {
            // Arrange
            _mockTransactionService.Setup(s => s.GetAllTransactionsAsync())
                       .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetTransactions();

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        [Fact]
        public async Task CreateTransaction_ReturnsCreatedResult_WithValidDto()
        {
            // Arrange
            var dto = new CreateTransactionDto
            {
                Amount = 150,
                Description = "Test transaction",
                Type = TransactionType.Income,
                Date = DateTime.Today
            };

            var createdTransaction = new Transaction
            {
                Id = 1,
                Amount = dto.Amount,
                Description = dto.Description,
                Type = dto.Type,
                Date = dto.Date,
                CreatedAt = DateTime.UtcNow
            };

            _mockTransactionService.Setup(s => s.CreateTransactionAsync(dto))
                       .ReturnsAsync(createdTransaction);

            // Act
            var result = await _controller.CreateTransaction(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedDto = Assert.IsType<TransactionResponseDto>(createdResult.Value);
            
            Assert.Equal(1, returnedDto.Id);
            Assert.Equal(150, returnedDto.Amount);
            Assert.Equal("Test transaction", returnedDto.Description);
            Assert.Equal("Income", returnedDto.Type);
        }

        [Fact]
        public async Task CreateTransaction_ReturnsBadRequest_WhenModelStateInvalid()
        {
            // Arrange
            var dto = new CreateTransactionDto(); // Invalid DTO
            _controller.ModelState.AddModelError("Amount", "Amount is required");

            // Act
            var result = await _controller.CreateTransaction(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateTransaction_ReturnsInternalServerError_WhenServiceThrows()
        {
            // Arrange
            var dto = new CreateTransactionDto
            {
                Amount = 150,
                Description = "Test",
                Type = TransactionType.Income,
                Date = DateTime.Today
            };

            _mockTransactionService.Setup(s => s.CreateTransactionAsync(dto))
                       .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreateTransaction(dto);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        [Fact]
        public async Task GetBalance_ReturnsOkResult_WithBalance()
        {
            // Arrange
            var expectedBalance = 1500.50m;
            _mockTransactionService.Setup(s => s.GetBalanceAsync())
                       .ReturnsAsync(expectedBalance);

            // Act
            var result = await _controller.GetBalance();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var balanceObject = okResult.Value;
            
            // Check if the returned object has a balance property
            var balanceProperty = balanceObject?.GetType().GetProperty("balance");
            Assert.NotNull(balanceProperty);
            Assert.Equal(expectedBalance, balanceProperty.GetValue(balanceObject));
        }

        [Fact]
        public async Task GetBalance_ReturnsInternalServerError_WhenServiceThrows()
        {
            // Arrange
            _mockTransactionService.Setup(s => s.GetBalanceAsync())
                       .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetBalance();

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusResult.StatusCode);
        }
    }
}