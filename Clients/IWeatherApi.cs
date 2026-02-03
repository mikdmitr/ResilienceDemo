using Refit;
using ResilienceDemo.Api.Models;

namespace ResilienceDemo.Api.Clients;

/// <summary>
/// Пример клиента для погодного API (симуляция)
/// Демонстрирует использование query параметров и заголовков
/// </summary>
public interface IWeatherApi
{
    /// <summary>
    /// Получить погоду по городу
    /// </summary>
    [Get("/weather")]
    Task<WeatherResponse> GetWeatherAsync(
        [Query] string city,
        [Header("X-Api-Key")] string apiKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить прогноз на несколько дней
    /// </summary>
    [Get("/forecast")]
    Task<List<WeatherResponse>> GetForecastAsync(
        [Query] string city,
        [Query] int days,
        [Header("X-Api-Key")] string apiKey,
        CancellationToken cancellationToken = default);
}
