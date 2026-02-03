using Refit;
using ResilienceDemo.Api.Models;

namespace ResilienceDemo.Api.Clients;

/// <summary>
/// Типизированный HTTP клиент с использованием Refit
/// Refit автоматически генерирует реализацию на основе интерфейса
/// </summary>
public interface IJsonPlaceholderApi
{
    /// <summary>
    /// Получить список пользователей
    /// </summary>
    [Get("/users")]
    Task<List<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить пользователя по ID
    /// </summary>
    [Get("/users/{id}")]
    Task<UserDto> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить список постов
    /// </summary>
    [Get("/posts")]
    Task<List<PostDto>> GetPostsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить посты пользователя
    /// </summary>
    [Get("/users/{userId}/posts")]
    Task<List<PostDto>> GetUserPostsAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Создать пост (демонстрация POST запроса)
    /// </summary>
    [Post("/posts")]
    Task<PostDto> CreatePostAsync([Body] PostDto post, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить пост (демонстрация PUT запроса)
    /// </summary>
    [Put("/posts/{id}")]
    Task<PostDto> UpdatePostAsync(int id, [Body] PostDto post, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить пост
    /// </summary>
    [Delete("/posts/{id}")]
    Task DeletePostAsync(int id, CancellationToken cancellationToken = default);
}
