namespace ResilienceDemo.Api.DelegateHandlersExample
{
    public class LoggingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Console.WriteLine("Request ----->>>>");

            var response = await base.SendAsync(request, cancellationToken);

            Console.WriteLine("Response <<<<-----");
            return response;
        }
    }
}
