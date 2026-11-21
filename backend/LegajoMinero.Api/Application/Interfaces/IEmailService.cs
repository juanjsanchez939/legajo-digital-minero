namespace LegajoMinero.Api.Application.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetAsync(
        string email,
        string link,
        CancellationToken cancellationToken = default);
}
