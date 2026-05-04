using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Refit;
using ResilienceDemo.Api.Clients;
using ResilienceDemo.Api.DelegateHandlersExample;
using ResilienceDemo.Api.Services;
using ResilienceDemo.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// 1. FLUENT VALIDATION - Валидация моделей
// ============================================
// Автоматическая регистрация всех валидаторов из сборки
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();

// Интеграция с ASP.NET Core (автоматическая валидация в контроллерах)
builder.Services.AddFluentValidationAutoValidation();

// ============================================
// 2. HTTP КЛИЕНТЫ И ПОЛИТИКИ УСТОЙЧИВОСТИ
// ============================================

// --- 2.1 Именованный HttpClient с Microsoft.Extensions.Http.Resilience ---
// Современная альтернатива ручной настройке Polly
builder.Services.AddHttpClient("JsonPlaceholder", client =>
{
    client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
})
// Добавляем стандартный resilience handler (retry, circuit breaker, timeout)
.AddStandardResilienceHandler(options =>
{
    // Настройка политики повторных попыток
    options.Retry.MaxRetryAttempts = 3;
    options.Retry.Delay = TimeSpan.FromMilliseconds(500);
    options.Retry.BackoffType = DelayBackoffType.Exponential;
    options.Retry.UseJitter = true; // Добавляет случайность для избежания "thundering herd"

    // Настройка Circuit Breaker
    options.CircuitBreaker.FailureRatio = 0.5; // 50% ошибок для размыкания
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
    options.CircuitBreaker.MinimumThroughput = 5; // Минимум запросов для анализа
    options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);

    // Настройка общего таймаута
    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);

    // Настройка таймаута на каждую попытку
    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
});

// --- 2.2 Refit - Типизированный HTTP клиент ---
// Refit автоматически генерирует реализацию интерфейса
builder.Services
    .AddRefitClient<IJsonPlaceholderApi>()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    // Можно добавить те же политики устойчивости
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 2;
        options.Retry.Delay = TimeSpan.FromMilliseconds(300);
    });

// --- 2.3 Пример кастомной политики для специфичного клиента ---
builder.Services.AddHttpClient("CustomResilience", client =>
{
    client.BaseAddress = new Uri("https://api.example.com");
})
// Добавляем кастомный делегат для логирования запросов и ответов
.AddHttpMessageHandler<LoggingHandler>()
// Добавляем кастомный делегат для валидации DTO после получения ответа
//.AddHttpMessageHandler<ValidationHandler<UserDto>>();
.AddResilienceHandler("custom-pipeline", builder =>
{
    // Полностью кастомный pipeline
    builder
        // Внешний таймаут на всю операцию
        .AddTimeout(TimeSpan.FromSeconds(30))
        // Retry с кастомной логикой
        .AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Exponential,
            ShouldHandle = args => ValueTask.FromResult(
                args.Outcome.Result?.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                args.Outcome.Result?.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        })
        // Circuit Breaker
        .AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        {
            FailureRatio = 0.3,
            SamplingDuration = TimeSpan.FromSeconds(60),
            MinimumThroughput = 10,
            BreakDuration = TimeSpan.FromMinutes(1)
        });
});

// ============================================
// 3. РЕГИСТРАЦИЯ СЕРВИСОВ
// ============================================
builder.Services.AddScoped<IPollyDemoService, PollyDemoService>();
builder.Services.AddScoped<IFluentResultsDemoService, FluentResultsDemoService>();
builder.Services.AddScoped<IResilientHttpService, ResilientHttpService>();

// ============================================
// 4. СТАНДАРТНАЯ КОНФИГУРАЦИЯ ASP.NET CORE
// ============================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Resilience Demo API",
        Version = "v1",
        Description = """
            Демонстрационный проект для изучения:
            - **Polly v8** - политики устойчивости (Retry, Circuit Breaker, Timeout)
            - **Microsoft.Extensions.Http.Resilience** - современная интеграция с HttpClient
            - **IHttpClientFactory** - управление HTTP клиентами
            - **FluentValidation** - валидация моделей
            - **Refit** - типизированные HTTP клиенты
            - **FluentResults** - функциональная обработка ошибок
            """
    });
});

// Добавляем проверку работоспособности
builder.Services.AddHealthChecks();

var app = builder.Build();

// ============================================
// 5. MIDDLEWARE PIPELINE
// ============================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Resilience Demo API v1");
        options.RoutePrefix = "swagger"; // Swagger доступен на /swagger
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Информация о запуске
app.Logger.LogInformation("""

    ====================================================================
                        RESILIENCE DEMO API
    
      Endpoints:
      • Swagger UI:        /swagger
      • Swagger JSON:      /swagger/v1/swagger.json
      • Health Check:      /health
    
      Demo Controllers:
      • /api/validationdemo  - FluentValidation примеры
      • /api/pollydemo       - Polly v8 политики
      • /api/fluentresultsdemo - FluentResults обработка ошибок
      • /api/httpclientdemo  - HTTP клиенты и Refit
    ====================================================================

    """);

app.Run();
