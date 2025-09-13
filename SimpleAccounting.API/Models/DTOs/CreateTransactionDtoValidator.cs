using FluentValidation;

namespace SimpleAccounting.API.Models.DTOs
{
    public class CreateTransactionDtoValidator : AbstractValidator<CreateTransactionDto>
    {
        public CreateTransactionDtoValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("金額は0より大きい値を入力してください");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("説明は必須です")
                .MaximumLength(200)
                .WithMessage("説明は200文字以内で入力してください");

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("有効な取引タイプを選択してください");

            RuleFor(x => x.Date)
                .NotEmpty()
                .WithMessage("日付は必須です")
                .LessThanOrEqualTo(DateTime.Today.AddDays(1))
                .WithMessage("未来の日付は入力できません");
        }
    }
}