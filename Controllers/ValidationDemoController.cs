using Microsoft.AspNetCore.Mvc;
using ResilienceDemo.Api.Models;

namespace ResilienceDemo.Api.Controllers;

/// <summary>
/// Демонстрация FluentValidation
/// Валидация автоматически применяется через DI
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ValidationDemoController : ControllerBase
{
    private readonly ILogger<ValidationDemoController> _logger;

    public ValidationDemoController(ILogger<ValidationDemoController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Создание заказа с автоматической валидацией FluentValidation
    /// POST api/validationdemo/orders
    /// </summary>
    /// <remarks>
    /// Пример валидного запроса:
    /// {
    ///     "customerName": "Иван Иванов",
    ///     "email": "ivan@company.com",
    ///     "phone": "+79001234567",
    ///     "amount": 1500,
    ///     "items": [
    ///         {
    ///             "productId": "AB-1234",
    ///             "productName": "Товар 1",
    ///             "quantity": 2,
    ///             "unitPrice": 750
    ///         }
    ///     ],
    ///     "shippingAddress": {
    ///         "street": "ул. Примерная, д. 1",
    ///         "city": "Москва",
    ///         "postalCode": "123456",
    ///         "country": "Россия"
    ///     }
    /// }
    /// </remarks>
    [HttpPost("orders")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult CreateOrder([FromBody] CreateOrderRequest request)
    {
        // Если мы дошли сюда - валидация прошла успешно
        // FluentValidation автоматически возвращает 400 при ошибках

        _logger.LogInformation(
            "Заказ создан для клиента {CustomerName}, сумма: {Amount}",
            request.CustomerName,
            request.Amount);

        return Ok(new
        {
            Message = "Заказ успешно создан",
            OrderId = Guid.NewGuid(),
            CustomerName = request.CustomerName,
            TotalAmount = request.Amount,
            ItemsCount = request.Items.Count
        });
    }

    /// <summary>
    /// Пример невалидного запроса для тестирования
    /// GET api/validationdemo/invalid-example
    /// </summary>
    [HttpGet("invalid-example")]
    public IActionResult GetInvalidExample()
    {
        return Ok(new CreateOrderRequest
        {
            CustomerName = "", // Пусто - ошибка
            Email = "invalid-email", // Неверный формат
            Phone = "123", // Неверный формат
            Amount = -100, // Отрицательное значение
            Items = new List<OrderItem>() // Пустой список
        });
    }

    /// <summary>
    /// Пример валидного запроса для тестирования
    /// GET api/validationdemo/valid-example
    /// </summary>
    [HttpGet("valid-example")]
    public IActionResult GetValidExample()
    {
        return Ok(new CreateOrderRequest
        {
            CustomerName = "Иван Иванов",
            Email = "ivan@company.com",
            Phone = "+79001234567",
            Amount = 2250,
            Items = new List<OrderItem>
            {
                new()
                {
                    ProductId = "AB-1234",
                    ProductName = "Ноутбук",
                    Quantity = 1,
                    UnitPrice = 1500
                },
                new()
                {
                    ProductId = "CD-5678",
                    ProductName = "Мышь",
                    Quantity = 3,
                    UnitPrice = 250
                }
            },
            ShippingAddress = new ShippingAddress
            {
                Street = "ул. Примерная, д. 1",
                City = "Москва",
                PostalCode = "123456",
                Country = "Россия"
            }
        });
    }
}
