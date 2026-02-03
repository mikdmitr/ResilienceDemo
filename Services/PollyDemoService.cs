using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace ResilienceDemo.Api.Services;

/// <summary>
/// Демонстрация классического Polly API (v7 стиль) и нового Polly v8 API
/// </summary>
public interface IPollyDemoService
{
    Task<string> ExecuteWithRetryAsync(Func<Task<string>> action);
    Task<string> ExecuteWithCircuitBreakerAsync(Func<Task<string>> action);
    Task<string> ExecuteWithTimeoutAsync(Func<CancellationToken, Task<string>> action);
    Task<string> ExecuteWithCombinedPoliciesAsync(Func<CancellationToken, Task<string>> action);
}

public class PollyDemoService : IPollyDemoService
{
    private readonly ILogger<PollyDemoService> _logger;

    // Polly v8: Новый ResiliencePipeline API
    private readonly ResiliencePipeline<string> _retryPipeline;
    private readonly ResiliencePipeline<string> _circuitBreakerPipeline;
    private readonly ResiliencePipeline<string> _timeoutPipeline;
    private readonly ResiliencePipeline<string> _combinedPipeline;

    public PollyDemoService(ILogger<PollyDemoService> logger)
    {
        _logger = logger;

        // ===== Polly v8: Retry Strategy =====
        // Политика повторных попыток с экспоненциальной задержкой
        _retryPipeline = new ResiliencePipelineBuilder<string>()
            .AddRetry(new RetryStrategyOptions<string>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true, // Добавляет случайность для избежания "thundering herd"
                ShouldHandle = new PredicateBuilder<string>()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutException>()
                    .HandleResult(r => string.IsNullOrEmpty(r)),
                OnRetry = args =>
                {
                    _logger.LogWarning(
                        "Retry attempt {AttemptNumber} after {Delay}ms. Exception: {Exception}",
                        args.AttemptNumber,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message ?? "No exception");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        // ===== Polly v8: Circuit Breaker Strategy =====
        // Прерыватель цепи - защита от каскадных сбоев
        _circuitBreakerPipeline = new ResiliencePipelineBuilder<string>()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<string>
            {
                FailureRatio = 0.5, // 50% ошибок для срабатывания
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 5, // Минимум 5 запросов для анализа
                BreakDuration = TimeSpan.FromSeconds(30), // Время в открытом состоянии
                ShouldHandle = new PredicateBuilder<string>()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutException>(),
                OnOpened = args =>
                {
                    _logger.LogError(
                        "Circuit breaker OPENED! Breaking for {BreakDuration}",
                        args.BreakDuration);
                    return ValueTask.CompletedTask;
                },
                OnClosed = args =>
                {
                    _logger.LogInformation("Circuit breaker CLOSED. Service recovered.");
                    return ValueTask.CompletedTask;
                },
                OnHalfOpened = args =>
                {
                    _logger.LogWarning("Circuit breaker HALF-OPENED. Testing service...");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        // ===== Polly v8: Timeout Strategy =====
        // Ограничение времени выполнения операции
        _timeoutPipeline = new ResiliencePipelineBuilder<string>()
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(10),
                OnTimeout = args =>
                {
                    _logger.LogWarning(
                        "Operation timed out after {Timeout}",
                        args.Timeout);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        // ===== Polly v8: Combined Pipeline =====
        // Комбинированный pipeline: Timeout -> Retry -> Circuit Breaker
        _combinedPipeline = new ResiliencePipelineBuilder<string>()
            // Внешний timeout - общее ограничение времени
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(30),
                OnTimeout = args =>
                {
                    _logger.LogError("Total operation timeout exceeded!");
                    return ValueTask.CompletedTask;
                }
            })
            // Retry с экспоненциальной задержкой
            .AddRetry(new RetryStrategyOptions<string>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(500),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder<string>()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutRejectedException>()
            })
            // Circuit Breaker
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<string>
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(60),
                MinimumThroughput = 10,
                BreakDuration = TimeSpan.FromMinutes(1)
            })
            // Внутренний timeout на каждую попытку
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(5)
            })
            .Build();
    }

    public async Task<string> ExecuteWithRetryAsync(Func<Task<string>> action)
    {
        return await _retryPipeline.ExecuteAsync(
            async (ct) => await action(),
            CancellationToken.None);
    }

    public async Task<string> ExecuteWithCircuitBreakerAsync(Func<Task<string>> action)
    {
        return await _circuitBreakerPipeline.ExecuteAsync(
            async (ct) => await action(),
            CancellationToken.None);
    }

    public async Task<string> ExecuteWithTimeoutAsync(Func<CancellationToken, Task<string>> action)
    {
        return await _timeoutPipeline.ExecuteAsync(
            async (ct) => await action(ct),
            CancellationToken.None);
    }

    public async Task<string> ExecuteWithCombinedPoliciesAsync(Func<CancellationToken, Task<string>> action)
    {
        return await _combinedPipeline.ExecuteAsync(
            async (ct) => await action(ct),
            CancellationToken.None);
    }
}
