using FluentValidation;
using System.Text.Json;

namespace ResilienceDemo.Api.DelegateHandlersExample
{
    public class ValidationHandler<T> : DelegatingHandler
    {
        private readonly IValidator<T> _validator;

        public ValidationHandler(IValidator<T> validator)
        {
            _validator = validator;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return response;

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            var model = JsonSerializer.Deserialize<T>(content);

            if (model == null)
                throw new ValidationException("Response is null or invalid JSON");

            var result = await _validator.ValidateAsync(model, cancellationToken);

            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }

            return response;
        }
    }
}
