using FluentValidation;
using ResilienceDemo.Api.Models;

namespace ResilienceDemo.Api.Validators;

/// <summary>
/// Валидатор для CreateOrderRequest с использованием FluentValidation
/// Демонстрирует различные правила валидации
/// </summary>
public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        // Базовая валидация строк
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Имя клиента обязательно")
            .MinimumLength(2).WithMessage("Имя должно содержать минимум 2 символа")
            .MaximumLength(100).WithMessage("Имя не должно превышать 100 символов");

        // Валидация email с кастомным сообщением
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен")
            .EmailAddress().WithMessage("Некорректный формат email")
            .Must(BeValidDomain).WithMessage("Email должен быть с корпоративного домена");

        // Валидация телефона с регулярным выражением
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Телефон обязателен")
            .Matches(@"^\+?[1-9]\d{10,14}$")
            .WithMessage("Телефон должен быть в международном формате");

        // Валидация числовых значений
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Сумма должна быть больше 0")
            .LessThanOrEqualTo(1_000_000).WithMessage("Сумма не может превышать 1 000 000");

        // Валидация коллекции
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Заказ должен содержать хотя бы один товар")
            .Must(items => items.Count <= 100)
            .WithMessage("Заказ не может содержать более 100 позиций");

        // Валидация каждого элемента коллекции
        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemValidator());

        // Условная валидация
        When(x => x.ShippingAddress != null, () =>
        {
            RuleFor(x => x.ShippingAddress!)
                .SetValidator(new ShippingAddressValidator());
        });

        // Кастомная валидация с бизнес-логикой
        RuleFor(x => x)
            .Must(BeValidTotalAmount)
            .WithMessage("Общая сумма товаров не совпадает с указанной суммой заказа");
    }

    private bool BeValidDomain(string email)
    {
        if (string.IsNullOrEmpty(email)) return false;
        // Пример: разрешаем только определенные домены
        var allowedDomains = new[] { "company.com", "gmail.com", "outlook.com" };
        var domain = email.Split('@').LastOrDefault();
        return domain != null && allowedDomains.Contains(domain.ToLower());
    }

    private bool BeValidTotalAmount(CreateOrderRequest request)
    {
        if (request.Items == null || !request.Items.Any()) return true;
        var calculatedTotal = request.Items.Sum(i => i.Quantity * i.UnitPrice);
        // Допускаем погрешность в 1%
        return Math.Abs(calculatedTotal - request.Amount) <= request.Amount * 0.01m;
    }
}

/// <summary>
/// Валидатор для позиции заказа
/// </summary>
public class OrderItemValidator : AbstractValidator<OrderItem>
{
    public OrderItemValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ID товара обязателен")
            .Matches(@"^[A-Z]{2,3}-\d{4,8}$")
            .WithMessage("ID товара должен быть в формате XX-0000 или XXX-00000000");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Название товара обязательно")
            .MaximumLength(200);

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Количество должно быть больше 0")
            .LessThanOrEqualTo(1000).WithMessage("Количество не может превышать 1000");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Цена должна быть больше 0");
    }
}

/// <summary>
/// Валидатор для адреса доставки
/// </summary>
public class ShippingAddressValidator : AbstractValidator<ShippingAddress>
{
    public ShippingAddressValidator()
    {
        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Улица обязательна")
            .MaximumLength(200);

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("Город обязателен")
            .MaximumLength(100);

        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("Почтовый индекс обязателен")
            .Matches(@"^\d{5,6}$").WithMessage("Некорректный формат почтового индекса");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Страна обязательна")
            .Must(BeValidCountry).WithMessage("Неподдерживаемая страна доставки");
    }

    private bool BeValidCountry(string country)
    {
        var supportedCountries = new[] { "Россия", "Беларусь", "Казахстан", "Russia", "Belarus", "Kazakhstan" };
        return supportedCountries.Contains(country, StringComparer.OrdinalIgnoreCase);
    }
}
