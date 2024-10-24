using Grpc.Core;
using Grpc.Core.Interceptors;
using Serilog;

namespace GrpcCrudExample.Interceptors;

public class ExceptionInterceptor : Interceptor
{
    public ExceptionInterceptor()
    {
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (Exception ex)
        {
            return HandleException<TResponse>(ex, context);
        }
    }

    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            await continuation(request, responseStream, context);
        }
        catch (Exception ex)
        {
            HandleException<TResponse>(ex, context);
        }
    }

    private TResponse HandleException<TResponse>(Exception ex, ServerCallContext context)
    {
        Log.Error(ex, "An error occurred");

        Status status;
        if (ex is ArgumentException)
        {
            status = new Status(StatusCode.InvalidArgument, ex.Message);
        }
        else if (ex is RpcException)
        {
            // RpcException is already handled by the gRPC framework
            throw ex;
        }
        else
        {
            // General exception handling
            status = new Status(StatusCode.Internal, "An internal error occurred");
        }

        throw new RpcException(status);
    }
}
