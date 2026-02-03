using Microsoft.AspNetCore.Mvc;
using ResilienceDemo.Api.Clients;
using ResilienceDemo.Api.Models;
using ResilienceDemo.Api.Services;

namespace ResilienceDemo.Api.Controllers;

/// <summary>
/// Демонстрация различных подходов к HTTP-клиентам:
/// 1. IHttpClientFactory с именованным клиентом
/// 2. Типизированный клиент через Refit
/// 3. Microsoft.Extensions.Http.Resilience
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HttpClientDemoController : ControllerBase
{
    private readonly IResilientHttpService _resilientService;
    private readonly IJsonPlaceholderApi _refitClient;
    private readonly ILogger<HttpClientDemoController> _logger;

    public HttpClientDemoController(
        IResilientHttpService resilientService,
        IJsonPlaceholderApi refitClient,
        ILogger<HttpClientDemoController> logger)
    {
        _resilientService = resilientService;
        _refitClient = refitClient;
        _logger = logger;
    }

    /// <summary>
    /// Получить пользователей через IHttpClientFactory с политиками устойчивости
    /// GET api/httpclientdemo/users/factory
    /// </summary>
    [HttpGet("users/factory")]
    public async Task<IActionResult> GetUsersViaFactory(CancellationToken ct)
    {
        _logger.LogInformation("Fetching users via IHttpClientFactory with resilience policies");

        var result = await _resilientService.GetUsersFromExternalApiAsync(ct);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new
            {
                Error = result.ErrorMessage,
                Method = "IHttpClientFactory + Microsoft.Extensions.Http.Resilience"
            });
        }

        return Ok(new
        {
            Data = result.Data,
            Count = result.Data?.Count,
            Method = "IHttpClientFactory + Microsoft.Extensions.Http.Resilience"
        });
    }

    /// <summary>
    /// Получить пользователя по ID через IHttpClientFactory
    /// GET api/httpclientdemo/users/factory/1
    /// </summary>
    [HttpGet("users/factory/{id:int}")]
    public async Task<IActionResult> GetUserViaFactory(int id, CancellationToken ct)
    {
        var result = await _resilientService.GetUserByIdAsync(id, ct);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { Error = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Получить пользователей через Refit (типизированный клиент)
    /// GET api/httpclientdemo/users/refit
    /// </summary>
    [HttpGet("users/refit")]
    public async Task<IActionResult> GetUsersViaRefit(CancellationToken ct)
    {
        _logger.LogInformation("Fetching users via Refit typed client");

        try
        {
            var users = await _refitClient.GetUsersAsync(ct);

            return Ok(new
            {
                Data = users,
                Count = users.Count,
                Method = "Refit typed HTTP client"
            });
        }
        catch (Refit.ApiException ex)
        {
            _logger.LogError(ex, "Refit API error");
            return StatusCode((int)ex.StatusCode, new
            {
                Error = ex.Message,
                Method = "Refit"
            });
        }
    }

    /// <summary>
    /// Получить пользователя по ID через Refit
    /// GET api/httpclientdemo/users/refit/1
    /// </summary>
    [HttpGet("users/refit/{id:int}")]
    public async Task<IActionResult> GetUserViaRefit(int id, CancellationToken ct)
    {
        try
        {
            var user = await _refitClient.GetUserByIdAsync(id, ct);
            return Ok(new
            {
                Data = user,
                Method = "Refit"
            });
        }
        catch (Refit.ApiException ex)
        {
            return StatusCode((int)ex.StatusCode, new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Получить посты пользователя через Refit
    /// GET api/httpclientdemo/users/1/posts
    /// </summary>
    [HttpGet("users/{userId:int}/posts")]
    public async Task<IActionResult> GetUserPosts(int userId, CancellationToken ct)
    {
        try
        {
            var posts = await _refitClient.GetUserPostsAsync(userId, ct);

            return Ok(new
            {
                UserId = userId,
                Posts = posts,
                Count = posts.Count
            });
        }
        catch (Refit.ApiException ex)
        {
            return StatusCode((int)ex.StatusCode, new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Создать пост через Refit (демонстрация POST)
    /// POST api/httpclientdemo/posts
    /// </summary>
    [HttpPost("posts")]
    public async Task<IActionResult> CreatePost([FromBody] PostDto post, CancellationToken ct)
    {
        try
        {
            var createdPost = await _refitClient.CreatePostAsync(post, ct);

            return Created($"/api/httpclientdemo/posts/{createdPost.Id}", new
            {
                Data = createdPost,
                Message = "Post created via Refit"
            });
        }
        catch (Refit.ApiException ex)
        {
            return StatusCode((int)ex.StatusCode, new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Сравнение методов - получить данные всеми способами
    /// GET api/httpclientdemo/compare
    /// </summary>
    [HttpGet("compare")]
    public async Task<IActionResult> CompareApproaches(CancellationToken ct)
    {
        var results = new Dictionary<string, object>();

        // 1. IHttpClientFactory
        var factoryResult = await _resilientService.GetUsersFromExternalApiAsync(ct);
        results["IHttpClientFactory"] = new
        {
            Success = factoryResult.Success,
            Count = factoryResult.Data?.Count ?? 0,
            Features = new[]
            {
                "Управление временем жизни HttpClient",
                "Интеграция с Microsoft.Extensions.Http.Resilience",
                "Именованные и типизированные клиенты"
            }
        };

        // 2. Refit
        try
        {
            var refitUsers = await _refitClient.GetUsersAsync(ct);
            results["Refit"] = new
            {
                Success = true,
                Count = refitUsers.Count,
                Features = new[]
                {
                    "Автогенерация клиента из интерфейса",
                    "Декларативный стиль",
                    "Сериализация/десериализация из коробки"
                }
            };
        }
        catch (Exception ex)
        {
            results["Refit"] = new { Success = false, Error = ex.Message };
        }

        return Ok(new
        {
            Comparison = results,
            Recommendation = "Используйте Refit для типизированных API клиентов с IHttpClientFactory для управления подключениями"
        });
    }
}
