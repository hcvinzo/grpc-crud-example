using System.Globalization;
using Grpc.Core;
using GrpcCrudExample.v1;
using Microsoft.AspNetCore.Authorization;
using Serilog;

namespace GrpcCrudExample.Services.v1;

public class AuthService : Auth.AuthBase
{
    private readonly JwtTokenService _jwtTokenService;

    public AuthService(JwtTokenService jwtTokenService)
    {
        _jwtTokenService = jwtTokenService;
    }
    /// <summary>
    /// Authenticate and generate a JWT token
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    /// <exception cref="RpcException"></exception>
    public override Task<AuthReply> Authenticate(AuthRequest request, ServerCallContext context)
    {
        Log.Information("Authetntication request for {Name} from {IpAddress}", request.Username, context.Peer);

        // Throws error if username or password is empty
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            throw new ArgumentException("Missing argument(s)");
        }

        // In a real application, you would validate the username and password here.
        // For this example, we'll just generate a token for any non-empty username.
        var token = _jwtTokenService.GenerateTokens(request.Username);
        return Task.FromResult(new AuthReply { Token = token.accessToken, RefreshToken = token.refreshToken });

    }

    /// <summary>
    /// Refresh the access token using a refresh token
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override Task<RefreshTokenResponse> RefreshToken(RefreshTokenRequest request, ServerCallContext context)
    {
        Log.Information("Refresh token request");

        // Throws error if refresh token is empty
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            throw new ArgumentException("Missing argument");
        }

        var (accessToken, refreshToken) = _jwtTokenService.RefreshToken(request.RefreshToken);
        return Task.FromResult(new RefreshTokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });

    }
}