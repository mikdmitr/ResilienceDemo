using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;
using Polly.Timeout;
using ResilienceDemo.Api.Services;

namespace ResilienceDemo.Api.Controllers;

/// <summary>
/// Демонстрация политик Polly v8
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PollyDemoController : ControllerBase
{
    private readonly IPollyDemoService _pollyService;
    private readonly ILogger<PollyDemoController> _logger;
    private static int _requestCount = 0;

    public PollyDemoController(
        IPollyDemoService pollyService,
        ILogger<PollyDemoController> logger)
    {
        _pollyService = pollyService;
        _logger = logger;
    }

    /// <summary>
    /// Демонстрация Retry политики
    /// Симулирует нестабильный сервис, который успешно отвечает после нескольких попыток
    /// GET api/pollydemo/retry
    /// </summary>
    [HttpGet("retry")]
    public async Task<IActionResult> DemoRetry([FromQuery] int failCount = 2)
    {
        var currentAttempt = 0;

        try
        {
            var result = await _pollyService.ExecuteWithRetryAsync(async () =>
            {
                currentAttempt++;
                _logger.LogInformation("Попытка {Attempt}", currentAttempt);

                // Симуляция: первые N попыток неудачны
                if (currentAttempt <= failCount)
                {
                    throw new HttpRequestException($"Simulated failure on attempt {currentAttempt}");
                }

                await Task.Delay(100); // Симуляция работы
                return $"Success on attempt {currentAttempt}";
            });

            return Ok(new
            {
                Message = result,
                TotalAttempts = currentAttempt,
                Policy = "Retry with exponential backoff"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Error = ex.Message,
                TotalAttempts = currentAttempt,
                Policy = "Retry exhausted all attempts"
            });
        }
    }

    /// <summary>
    /// Демонстрация Circuit Breaker
    /// После нескольких ошибок цепь размыкается и запросы сразу отклоняются
    /// GET api/pollydemo/circuit-breaker
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [HttpGet("circuit-breaker")]
    public async Task<IActionResult> DemoCircuitBreaker([FromQuery] bool simulateFailure = false)
    {
        try
        {
            var result = await _pollyService.ExecuteWithCircuitBreakerAsync(async () =>
            {
                if (simulateFailure)
                {
                    throw new HttpRequestException("Simulated service failure");
                }

                await Task.Delay(50);
                return "Service responded successfully";
            });

            return Ok(new
            {
                Message = result,
                Policy = "Circuit Breaker",
                Status = "Closed (healthy)"
            });
        }
        catch (BrokenCircuitException ex)
        {
            return StatusCode(503, new
            {
                Error = "Circuit is OPEN - service temporarily unavailable",
                Details = ex.Message,
                Policy = "Circuit Breaker",
                Status = "Open (blocking requests)"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Error = ex.Message,
                Policy = "Circuit Breaker"
            });
        }
    }

    /// <summary>
    /// Демонстрация Timeout политики
    /// GET api/pollydemo/timeout
    /// </summary>
    [HttpGet("timeout")]
    public async Task<IActionResult> DemoTimeout([FromQuery] int delayMs = 500)
    {
        try
        {
            var result = await _pollyService.ExecuteWithTimeoutAsync(async ct =>
            {
                _logger.LogInformation("Starting operation with {Delay}ms delay", delayMs);

                // Симуляция долгой операции
                await Task.Delay(delayMs, ct);

                return $"Completed in {delayMs}ms";
            });

            return Ok(new
            {
                Message = result,
                RequestedDelay = delayMs,
                Policy = "Timeout (10 seconds limit)"
            });
        }
        catch (TimeoutRejectedException)
        {
            return StatusCode(408, new
            {
                Error = "Operation timed out",
                RequestedDelay = delayMs,
                Policy = "Timeout exceeded"
            });
        }
    }

    /// <summary>
    /// Демонстрация комбинированных политик
    /// Timeout -> Retry -> Circuit Breaker
    /// GET api/pollydemo/combined
    /// </summary>
    [HttpGet("combined")]
    public async Task<IActionResult> DemoCombinedPolicies(
        [FromQuery] bool simulateFailure = false,
        [FromQuery] int delayMs = 100)
    {
        _requestCount++;
        var requestId = _requestCount;

        try
        {
            var result = await _pollyService.ExecuteWithCombinedPoliciesAsync(async ct =>
            {
                _logger.LogInformation("Request {RequestId}: Processing...", requestId);

                await Task.Delay(delayMs, ct);

                if (simulateFailure)
                {
                    throw new HttpRequestException($"Request {requestId} failed");
                }

                return $"Request {requestId} completed successfully";
            });

            return Ok(new
            {
                RequestId = requestId,
                Message = result,
                Policy = "Combined: Timeout -> Retry -> CircuitBreaker -> Timeout"
            });
        }
        catch (BrokenCircuitException)
        {
            return StatusCode(503, new
            {
                RequestId = requestId,
                Error = "Service unavailable - circuit breaker is open",
                Policy = "Circuit Breaker triggered"
            });
        }
        catch (TimeoutRejectedException)
        {
            return StatusCode(408, new
            {
                RequestId = requestId,
                Error = "Request timed out",
                Policy = "Timeout triggered"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                RequestId = requestId,
                Error = ex.Message
            });
        }
    }
}
