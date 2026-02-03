using Microsoft.AspNetCore.Mvc;
using ResilienceDemo.Api.Models;
using ResilienceDemo.Api.Services;

namespace ResilienceDemo.Api.Controllers;

/// <summary>
/// Демонстрация FluentResults - функциональная обработка ошибок
/// Альтернатива исключениям для бизнес-логики
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FluentResultsDemoController : ControllerBase
{
    private readonly IFluentResultsDemoService _service;
    private readonly ILogger<FluentResultsDemoController> _logger;

    public FluentResultsDemoController(
        IFluentResultsDemoService service,
        ILogger<FluentResultsDemoController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Получить пользователя по ID
    /// Демонстрация обработки Result<T>
    /// GET api/fluentresultsdemo/users/1
    /// </summary>
    [HttpGet("users/{id:int}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _service.GetUserAsync(id);

        // Способ 1: Использование Match для обработки результата
        return result.ToResult().Match<IActionResult>(
            onSuccess: () => Ok(new
            {
                Data = result.Value,
                Successes = result.Successes.Select(s => s.Message)
            }),
            onFailure: () =>
            {
                var statusCode = GetStatusCodeFromErrors(result.Errors);
                return StatusCode(statusCode, new
                {
                    Errors = result.Errors.Select(e => new
                    {
                        Message = e.Message,
                        Type = e.Metadata.GetValueOrDefault("ErrorType")
                    })
                });
            }
        );
    }

    /// <summary>
    /// Получить всех пользователей
    /// GET api/fluentresultsdemo/users
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var result = await _service.GetUsersAsync();

        // Способ 2: Использование IsSuccess/IsFailed
        if (result.IsSuccess)
        {
            return Ok(new
            {
                Data = result.Value,
                Count = result.Value?.Count,
                Message = result.Successes.FirstOrDefault()?.Message
            });
        }

        return NotFound(new
        {
            Errors = result.Errors.Select(e => e.Message)
        });
    }

    /// <summary>
    /// Создать пользователя
    /// Демонстрация накопления ошибок валидации
    /// POST api/fluentresultsdemo/users
    /// </summary>
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] UserDto user)
    {
        var result = await _service.CreateUserAsync(user);

        if (result.IsFailed)
        {
            var statusCode = GetStatusCodeFromErrors(result.Errors);

            // Группировка ошибок по типу
            var errorsByType = result.Errors
                .GroupBy(e => e.Metadata.GetValueOrDefault("ErrorType")?.ToString() ?? "Unknown")
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.Message).ToList()
                );

            return StatusCode(statusCode, new
            {
                Errors = errorsByType,
                TotalErrors = result.Errors.Count
            });
        }

        return Created($"/api/fluentresultsdemo/users/{result.Value.Id}", new
        {
            Data = result.Value,
            Message = result.Successes.FirstOrDefault()?.Message
        });
    }

    /// <summary>
    /// Удалить пользователя
    /// Демонстрация Result без значения
    /// DELETE api/fluentresultsdemo/users/1
    /// </summary>
    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var result = await _service.DeleteUserAsync(id);

        // Способ 3: Условные проверки
        if (result.HasError<NotFoundError>())
        {
            return NotFound(new { Error = result.Errors.First().Message });
        }

        if (result.HasError<BusinessRuleError>())
        {
            return UnprocessableEntity(new { Error = result.Errors.First().Message });
        }

        if (result.HasError<ValidationError>())
        {
            return BadRequest(new { Error = result.Errors.First().Message });
        }

        if (result.IsSuccess)
        {
            return Ok(new
            {
                Message = result.Successes.FirstOrDefault()?.Message ?? "Deleted successfully"
            });
        }

        return StatusCode(500, new { Error = "Unknown error" });
    }

    private int GetStatusCodeFromErrors(IEnumerable<FluentResults.IError> errors)
    {
        var firstError = errors.FirstOrDefault();
        if (firstError?.Metadata.TryGetValue("StatusCode", out var statusCode) == true)
        {
            return Convert.ToInt32(statusCode);
        }
        return 500;
    }
}

// Extension методы для Result (FluentResults)
public static class ResultExtensions
{
    public static T Match<T>(
        this FluentResults.Result result,
        Func<T> onSuccess,
        Func<T> onFailure)
    {
        return result.IsSuccess ? onSuccess() : onFailure();
    }
}
