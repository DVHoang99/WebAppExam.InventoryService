using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Configuration;
using WebAppExam.InventoryService.Infrastructure.Constants;

namespace WebAppExam.InventoryService.Infrastructure.Common;

public class InternalApiKeyInterceptor : Interceptor
{
    private readonly string _apiKey;

    public InternalApiKeyInterceptor(IConfiguration configuration)
    {
        _apiKey = configuration[CommonConstants.InternalApiKeyConfigPath] 
                  ?? throw new InvalidOperationException($"Configuration {CommonConstants.InternalApiKeyConfigPath} is missing.");
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var metadata = context.Options.Headers ?? new Metadata();
        metadata.Add(CommonConstants.InternalKeyHeader, _apiKey);

        var newOptions = context.Options.WithHeaders(metadata);
        var newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, newOptions);

        return continuation(request, newContext);
    }
}
