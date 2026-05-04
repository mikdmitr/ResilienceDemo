using FluentValidation;
using ResilienceDemo.Api.Models;

namespace ResilienceDemo.Api.Validators
{
    public class UserDtoValidator: AbstractValidator<UserDto>
    {
        public UserDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Имя обязательно")
                .MinimumLength(2).WithMessage("Имя должно содержать минимум 2 символа")
                .MaximumLength(100).WithMessage("Имя не должно превышать 100 символов");
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email обязателен")
                .EmailAddress().WithMessage("Некорректный формат email");
            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Телефон обязателен")
                .Matches(@"^\+?[1-9]\d{10,14}$")
                .WithMessage("Телефон должен быть в международном формате");
        }
    }
}
