using FluentResults;
using ResilienceDemo.Api.Models;

namespace ResilienceDemo.Api.Services;

/// <summary>
/// Демонстрация FluentResults - современная альтернатива исключениям
/// для обработки ошибок и успешных результатов
/// </summary>
public interface IFluentResultsDemoService
{
    Task<Result<UserDto>> GetUserAsync(int id);
    Task<Result<List<UserDto>>> GetUsersAsync();
    Task<Result> DeleteUserAsync(int id);
    Task<Result<UserDto>> CreateUserAsync(UserDto user);
}

public class FluentResultsDemoService : IFluentResultsDemoService
{
    private readonly ILogger<FluentResultsDemoService> _logger;
    private readonly List<UserDto> _users = new()
    {
        new UserDto { Id = 1, Name = "Иван Иванов", Email = "ivan@company.com", Phone = "+79001234567" },
        new UserDto { Id = 2, Name = "Мария Петрова", Email = "maria@company.com", Phone = "+79001234568" },
        new UserDto { Id = 3, Name = "Алексей Сидоров", Email = "alexey@company.com", Phone = "+79001234569" }
    };

    public FluentResultsDemoService(ILogger<FluentResultsDemoService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Получение пользователя с использованием Result<T>
    /// </summary>
    public Task<Result<UserDto>> GetUserAsync(int id)
    {
        // Симуляция различных сценариев
        if (id <= 0)
        {
            return Task.FromResult(Result.Fail<UserDto>(
                new ValidationError("ID пользователя должен быть положительным числом")));
        }

        var user = _users.FirstOrDefault(u => u.Id == id);

        if (user == null)
        {
            return Task.FromResult(Result.Fail<UserDto>(
                new NotFoundError($"Пользователь с ID {id} не найден")));
        }

        _logger.LogInformation("Пользователь {UserId} успешно получен", id);

        return Task.FromResult(Result.Ok(user)
            .WithSuccess(new SuccessWithMessage($"Пользователь {user.Name} найден")));
    }

    /// <summary>
    /// Получение списка пользователей
    /// </summary>
    public Task<Result<List<UserDto>>> GetUsersAsync()
    {
        if (!_users.Any())
        {
            return Task.FromResult(Result.Fail<List<UserDto>>(
                new NotFoundError("Список пользователей пуст")));
        }

        return Task.FromResult(Result.Ok(_users)
            .WithSuccess($"Найдено {_users.Count} пользователей"));
    }

    /// <summary>
    /// Удаление пользователя - возвращает Result без значения
    /// </summary>
    public Task<Result> DeleteUserAsync(int id)
    {
        if (id <= 0)
        {
            return Task.FromResult(Result.Fail(
                new ValidationError("ID пользователя должен быть положительным числом")));
        }

        var user = _users.FirstOrDefault(u => u.Id == id);

        if (user == null)
        {
            return Task.FromResult(Result.Fail(
                new NotFoundError($"Пользователь с ID {id} не найден")));
        }

        // Проверка бизнес-правил
        if (user.Id == 1)
        {
            return Task.FromResult(Result.Fail(
                new BusinessRuleError("Нельзя удалить администратора системы")));
        }

        _users.Remove(user);
        _logger.LogInformation("Пользователь {UserId} удален", id);

        return Task.FromResult(Result.Ok()
            .WithSuccess($"Пользователь {user.Name} успешно удален"));
    }

    /// <summary>
    /// Создание пользователя с комплексной валидацией
    /// </summary>
    public Task<Result<UserDto>> CreateUserAsync(UserDto user)
    {
        var result = Result.Ok();

        // Накопление ошибок (а не fail-fast)
        if (string.IsNullOrWhiteSpace(user.Name))
        {
            result = result.WithError(new ValidationError("Имя пользователя обязательно"));
        }

        if (string.IsNullOrWhiteSpace(user.Email))
        {
            result = result.WithError(new ValidationError("Email обязателен"));
        }
        else if (_users.Any(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
        {
            result = result.WithError(new ConflictError($"Пользователь с email {user.Email} уже существует"));
        }

        if (result.IsFailed)
        {
            return Task.FromResult(result.ToResult<UserDto>());
        }

        // Создание пользователя
        user.Id = _users.Max(u => u.Id) + 1;
        _users.Add(user);

        _logger.LogInformation("Создан пользователь {UserId}: {UserName}", user.Id, user.Name);

        return Task.FromResult(Result.Ok(user)
            .WithSuccess($"Пользователь {user.Name} успешно создан с ID {user.Id}"));
    }
}

// ===== Кастомные типы ошибок =====

/// <summary>
/// Ошибка валидации
/// </summary>
public class ValidationError : Error
{
    public ValidationError(string message) : base(message)
    {
        Metadata.Add("ErrorType", "Validation");
        Metadata.Add("StatusCode", 400);
    }
}

/// <summary>
/// Ресурс не найден
/// </summary>
public class NotFoundError : Error
{
    public NotFoundError(string message) : base(message)
    {
        Metadata.Add("ErrorType", "NotFound");
        Metadata.Add("StatusCode", 404);
    }
}

/// <summary>
/// Нарушение бизнес-правила
/// </summary>
public class BusinessRuleError : Error
{
    public BusinessRuleError(string message) : base(message)
    {
        Metadata.Add("ErrorType", "BusinessRule");
        Metadata.Add("StatusCode", 422);
    }
}

/// <summary>
/// Конфликт данных
/// </summary>
public class ConflictError : Error
{
    public ConflictError(string message) : base(message)
    {
        Metadata.Add("ErrorType", "Conflict");
        Metadata.Add("StatusCode", 409);
    }
}

/// <summary>
/// Успешный результат с сообщением
/// </summary>
public class SuccessWithMessage : Success
{
    public SuccessWithMessage(string message) : base(message) { }
}
