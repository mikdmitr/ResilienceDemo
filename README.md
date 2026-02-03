# Resilience Demo API

Демонстрационный проект для изучения паттернов устойчивости и обработки ошибок в .NET 10

## ?? Цель проекта

Демонстрация современных подходов к созданию устойчивых API:
- **Polly v8** - политики устойчивости (Retry, Circuit Breaker, Timeout)
- **Microsoft.Extensions.Http.Resilience** - интеграция с HttpClient
- **FluentValidation** - валидация моделей
- **Refit** - типизированные HTTP клиенты
- **FluentResults** - функциональная обработка ошибок

## ?? Быстрый старт

### Предварительные требования
- .NET 10 SDK
- Visual Studio 2026 или VS Code

### Запуск приложения

1. Клонируйте репозиторий
2. Откройте решение в Visual Studio
3. Запустите проект (F5)
4. Откройте браузер: `https://localhost:7074/swagger`

## ?? API Endpoints

### Swagger UI
- **URL**: `https://localhost:7074/swagger`
- **JSON**: `https://localhost:7074/swagger/v1/swagger.json`

### Health Check
- **URL**: `https://localhost:7074/health`

### Demo Controllers

| Endpoint | Описание |
|----------|----------|
| `/api/validationdemo` | FluentValidation примеры |
| `/api/pollydemo` | Polly v8 политики устойчивости |
| `/api/fluentresultsdemo` | FluentResults обработка ошибок |
| `/api/httpclientdemo` | HTTP клиенты и Refit |

## ?? Технологии

### Основные библиотеки
- **ASP.NET Core 10** - веб-фреймворк
- **Polly v8** - resilience patterns
- **Microsoft.Extensions.Http.Resilience** - стандартные политики для HttpClient
- **Refit** - декларативные HTTP клиенты
- **FluentValidation** - валидация запросов
- **FluentResults** - Result pattern
- **Swashbuckle** - OpenAPI/Swagger документация

### Паттерны устойчивости

#### 1. Retry (Повторные попытки)
```csharp
- Экспоненциальная задержка
- Jitter для избежания "thundering herd"
- Настраиваемые условия повтора
```

#### 2. Circuit Breaker (Прерыватель цепи)
```csharp
- Защита от каскадных сбоев
- Автоматическое восстановление
- Мониторинг состояния
```

#### 3. Timeout (Таймауты)
```csharp
- Глобальный таймаут
- Таймаут на попытку
- Комбинированные таймауты
```

#### 4. Combined Policies (Комбинированные политики)
```csharp
- Timeout -> Retry -> Circuit Breaker -> Timeout
- Оптимальная последовательность политик
```

## ?? Структура проекта

```
ResilienceDemo.Api/
??? Controllers/           # API контроллеры
?   ??? PollyDemoController.cs
?   ??? ValidationDemoController.cs
?   ??? FluentResultsDemoController.cs
?   ??? HttpClientDemoController.cs
??? Services/             # Бизнес-логика
?   ??? PollyDemoService.cs
?   ??? FluentResultsDemoService.cs
?   ??? ResilientHttpService.cs
??? Clients/              # HTTP клиенты
?   ??? IJsonPlaceholderApi.cs
?   ??? IWeatherApi.cs
??? Validators/           # FluentValidation валидаторы
?   ??? CreateOrderRequestValidator.cs
??? Models/               # DTO модели
?   ??? CreateOrderRequest.cs
?   ??? WeatherResponse.cs
??? Program.cs           # Конфигурация приложения
```

## ?? Git Issues

Если возникли проблемы с Git (encoding, ownership), см. [GIT-ENCODING-FIX.md](GIT-ENCODING-FIX.md)

Быстрое решение:
```powershell
.\fix-git-complete.ps1
```

## ?? Примеры использования

### Polly Demo - Retry Policy
```http
GET /api/pollydemo/retry?failCount=2
```

### Polly Demo - Circuit Breaker
```http
GET /api/pollydemo/circuit-breaker?shouldFail=true
```

### Validation Demo
```http
POST /api/validationdemo/create-order
Content-Type: application/json

{
  "productName": "Test Product",
  "quantity": 5,
  "price": 99.99,
  "email": "test@example.com"
}
```

## ?? Обучающие материалы

### Polly v8
- [Официальная документация](https://www.pollydocs.org/)
- Основные изменения в v8
- ResiliencePipeline API

### Resilience Patterns
- Retry Strategy
- Circuit Breaker Pattern
- Timeout Patterns
- Bulkhead Isolation

### Best Practices
- HTTP клиенты и IHttpClientFactory
- Валидация входных данных
- Обработка ошибок с Result pattern

## ?? Мониторинг и логирование

Проект использует стандартное логирование ASP.NET Core:
- Retry attempts с задержками
- Circuit Breaker state changes
- Timeout events
- Validation errors

## ?? Лицензия

Демонстрационный проект для образовательных целей.

## ?? Вклад в проект

Проект создан для обучения. Не стесняйтесь экспериментировать!

## ?? Контакты

Создано как часть курса OTUS - Профессиональная разработка на .NET
