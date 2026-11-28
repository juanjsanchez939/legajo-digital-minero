using LegajoMinero.Api.Application.DTOs;
using LegajoMinero.Api.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LegajoMinero.Api.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("registrar")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Registrar(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(
            request,
            ClientIp,
            Device,
            cancellationToken);

        return ToActionResult(result, StatusCodes.Status201Created);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(
            request,
            ClientIp,
            Device,
            cancellationToken);

        return ToActionResult(result, StatusCodes.Status200OK);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshAsync(
            request,
            ClientIp,
            Device,
            cancellationToken);

        return ToActionResult(result, StatusCodes.Status200OK);
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LogoutAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("solicitar-recuperacion")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> SolicitarRecuperacion(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _authService.RequestPasswordResetAsync(request, cancellationToken);
        return Accepted();
    }

    [HttpPost("restablecer-contrasena")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RestablecerContrasena(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.ResetPasswordAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    private string? ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();

    private string? Device
    {
        get
        {
            var userAgent = Request.Headers.UserAgent.ToString();
            if (string.IsNullOrWhiteSpace(userAgent))
            {
                return null;
            }

            return userAgent.Length <= 255 ? userAgent : userAgent[..255];
        }
    }

    private IActionResult ToActionResult(AuthResult result, int successStatusCode)
    {
        if (result.Data is not null)
        {
            return StatusCode(successStatusCode, result.Data);
        }

        if (result.RetryAfterSeconds is { } retryAfter)
        {
            Response.Headers.RetryAfter = retryAfter.ToString();
        }

        return Problem(
            statusCode: result.StatusCode,
            title: result.Title,
            detail: result.Error);
    }

    private IActionResult ToActionResult(OperationResult result)
    {
        if (result.StatusCode is StatusCodes.Status204NoContent)
        {
            return NoContent();
        }

        return Problem(
            statusCode: result.StatusCode,
            title: result.Title,
            detail: result.Error);
    }
}
