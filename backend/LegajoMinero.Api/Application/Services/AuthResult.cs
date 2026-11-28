using LegajoMinero.Api.Application.DTOs;

namespace LegajoMinero.Api.Application.Services;

public sealed record AuthResult(
    AuthResponse? Data,
    int StatusCode,
    string? Title,
    string? Error,
    int? RetryAfterSeconds = null)
{
    public static AuthResult Ok(AuthResponse data, int statusCode = 200) =>
        new(data, statusCode, null, null);

    public static AuthResult Fail(
        int statusCode,
        string error,
        string? title = null,
        int? retryAfterSeconds = null) =>
        new(null, statusCode, title, error, retryAfterSeconds);
}

public sealed record OperationResult(int StatusCode, string? Title = null, string? Error = null);
