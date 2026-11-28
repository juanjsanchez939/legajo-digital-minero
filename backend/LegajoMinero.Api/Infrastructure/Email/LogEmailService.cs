using LegajoMinero.Api.Application.Interfaces;

namespace LegajoMinero.Api.Infrastructure.Email;

/// <summary>
/// Implementación de desarrollo: registra el enlace en el log estructurado.
/// En producción se reemplaza por SMTP o SendGrid.
/// </summary>
public sealed class LogEmailService : IEmailService
{
    private readonly ILogger<LogEmailService> _logger;

    public LogEmailService(ILogger<LogEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendPasswordResetAsync(
        string email,
        string link,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Enlace de recuperación de contraseña para {Email}: {Link}",
            email,
            link);

        return Task.CompletedTask;
    }
}
