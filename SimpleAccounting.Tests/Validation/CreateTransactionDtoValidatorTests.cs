using FluentValidation.TestHelper;
using SimpleAccounting.API.Models;
using SimpleAccounting.API.Models.DTOs;
using Xunit;

namespace SimpleAccounting.Tests.Validation
{
    public class CreateTransactionDtoValidatorTests
    {
        private readonly CreateTransactionDtoValidator _validator;

        public CreateTransactionDtoValidatorTests()
        {
            _validator = new CreateTransactionDtoValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Amount_Is_Zero()
        {
            // Arrange
            var dto = new CreateTransactionDto { Amount = 0 };

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Amount)
                  .WithErrorMessage("金額は0より大きい値を入力してください");
        }

        [Fact]
        public void Should_Have_Error_When_Amount_Is_Negative()
        {
            // Arrange
            var dto = new CreateTransactionDto { Amount = -100 };

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Amount)
                  .WithErrorMessage("金額は0より大きい値を入力してください");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Amount_Is_Positive()
        {
            // Arrange
            var dto = new CreateTransactionDto 
            { 
                Amount = 100.50m,
                Description = "Valid description",
                Type = TransactionType.Income,
                Date = DateTime.Today
            };

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Amount);
        }

        [Fact]
        public void Should_Have_Error_When_Description_Is_Empty()
        {
            // Arrange
            var dto = new CreateTransactionDto { Description = "" };

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("説明は必須です");
        }

        [Fact]
        public void Should_Have_Error_When_Description_Is_Null()
        {
            // Arrange
            var dto = new CreateTransactionDto { Description = null! };

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("説明は必須です");
        }

        [Fact]
        public void Should_Have_Error_When_Description_Exceeds_MaxLength()
        {
            // Arrange
            var longDescription = new string('a', 201); // 201 characters
            var dto = new CreateTransactionDto { Description = longDescription };

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("説明は200文字以内で入力してください");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Description_Is_Valid()
        {
            // Arrange
            var dto = new CreateTransactionDto 
            { 
                Amount = 100,
                Description = "Valid description",
                Type = TransactionType.Income,
                Date = DateTime.Today
            };

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Should_Have_Error_When_Type_Is_Invalid()
        {
            // Arrange
            var dto = new CreateTransactionDto { Type = (TransactionType)999 };

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Type)
                  .WithErrorMessage("有効な取引タイプを選択してください");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Type_Is_Income()
        {
            // Arrange
            var dto = new CreateTransactionDto 
            { 
                Amount = 100,
                Description = "Valid description",
                Type = TransactionType.Income,
                Date = DateTime.Today
            };

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Type);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Type_Is_Expense()
        {
            // Arrange
            var dto = new CreateTransactionDto 
            { 
                Amount = 100,
                Description = "Valid description",
                Type = TransactionType.Expense,
                Date = DateTime.Today
            };

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Type);
        }

        [Fact]
        public void Should_Have_Error_When_Date_Is_Default()
        {
            // Arrange
            var dto = new CreateTransactionDto { Date = default(DateTime) };

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Date)
                  .WithErrorMessage("日付は必須です");
        }

        [Fact]
        public void Should_Have_Error_When_Date_Is_Future()
        {
            // Arrange
            var futureDate = DateTime.Today.AddDays(2);
            var dto = new CreateTransactionDto { Date = futureDate };

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Date)
                  .WithErrorMessage("未来の日付は入力できません");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Date_Is_Today()
        {
            // Arrange
            var dto = new CreateTransactionDto 
            { 
                Amount = 100,
                Description = "Valid description",
                Type = TransactionType.Income,
                Date = DateTime.Today
            };

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Date);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Date_Is_Yesterday()
        {
            // Arrange
            var dto = new CreateTransactionDto 
            { 
                Amount = 100,
                Description = "Valid description",
                Type = TransactionType.Income,
                Date = DateTime.Today.AddDays(-1)
            };

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Date);
        }

        [Fact]
        public void Should_Pass_Validation_When_All_Properties_Are_Valid()
        {
            // Arrange
            var dto = new CreateTransactionDto
            {
                Amount = 150.75m,
                Description = "Valid transaction description",
                Type = TransactionType.Income,
                Date = DateTime.Today
            };

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}