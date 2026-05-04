using FluentValidation;
using ResilienceDemo.Api.Models;
using ResilienceDemo.Api.Validators;

namespace ResilienceDemo.Api.Services;

/// <summary>
/// Сервис демонстрирующий работу с IHttpClientFactory
/// и политиками устойчивости через Microsoft.Extensions.Http.Resilience
/// </summary>
public interface IResilientHttpService
{
    Task<ExternalApiResponse<List<UserDto>>> GetUsersFromExternalApiAsync(CancellationToken ct = default);
    Task<ExternalApiResponse<UserDto>> GetUserByIdAsync(int id, CancellationToken ct = default);
    Task<ExternalApiResponse<List<PostDto>>> GetPostsAsync(CancellationToken ct = default);
}

public class ResilientHttpService : IResilientHttpService
{
    // Именованный HttpClient с настроенными политиками устойчивости
    private readonly HttpClient _httpClient;
    private readonly ILogger<ResilientHttpService> _logger;
    private readonly UserDtoValidator _userDtoValidator;

    public ResilientHttpService(
        IHttpClientFactory httpClientFactory,
        ILogger<ResilientHttpService> logger,
        UserDtoValidator userDtoValidator)
    {
        // Получаем клиент с предварительно настроенными политиками
        _httpClient = httpClientFactory.CreateClient("JsonPlaceholder");
        _logger = logger;
        _userDtoValidator = userDtoValidator;
    }

    public async Task<ExternalApiResponse<List<UserDto>>> GetUsersFromExternalApiAsync(
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Запрос списка пользователей из внешнего API");

            var response = await _httpClient.GetAsync("/users", ct);
            response.EnsureSuccessStatusCode();

            var users = await response.Content.ReadFromJsonAsync<List<UserDto>>(ct);

            return new ExternalApiResponse<List<UserDto>>
            {
                Success = true,
                Data = users,
                StatusCode = (int)response.StatusCode
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка HTTP запроса");
            return new ExternalApiResponse<List<UserDto>>
            {
                Success = false,
                ErrorMessage = $"HTTP Error: {ex.Message}",
                StatusCode = (int?)ex.StatusCode ?? 500
            };
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Таймаут запроса");
            return new ExternalApiResponse<List<UserDto>>
            {
                Success = false,
                ErrorMessage = "Request timed out",
                StatusCode = 408
            };
        }
    }

    public async Task<ExternalApiResponse<UserDto>> GetUserByIdAsync(
        int id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Запрос пользователя {UserId} из внешнего API", id);

            var response = await _httpClient.GetAsync($"/users/{id}", ct);

            if (!response.IsSuccessStatusCode)
            {
                return new ExternalApiResponse<UserDto>
                {
                    Success = false,
                    ErrorMessage = $"User not found: {response.StatusCode}",
                    StatusCode = (int)response.StatusCode
                };
            }

            var user = await response.Content.ReadFromJsonAsync<UserDto>(ct);

            await _userDtoValidator.ValidateAndThrowAsync(user, ct);

            return new ExternalApiResponse<UserDto>
            {
                Success = true,
                Data = user,
                StatusCode = (int)response.StatusCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка получения пользователя {UserId}", id);
            return new ExternalApiResponse<UserDto>
            {
                Success = false,
                ErrorMessage = ex.Message,
                StatusCode = 500
            };
        }
    }

    public async Task<ExternalApiResponse<List<PostDto>>> GetPostsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Запрос постов из внешнего API");

            var posts = await _httpClient.GetFromJsonAsync<List<PostDto>>("/posts", ct);

            return new ExternalApiResponse<List<PostDto>>
            {
                Success = true,
                Data = posts,
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка получения постов");
            return new ExternalApiResponse<List<PostDto>>
            {
                Success = false,
                ErrorMessage = ex.Message,
                StatusCode = 500
            };
        }
    }
}
