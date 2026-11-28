using LegajoMinero.Api.Application.Interfaces;
using LegajoMinero.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace LegajoMinero.Tests;

public sealed class TestEmailService : IEmailService
{
    public string? LastEmail { get; private set; }
    public string? LastLink { get; private set; }

    public Task SendPasswordResetAsync(
        string email,
        string link,
        CancellationToken cancellationToken = default)
    {
        LastEmail = email;
        LastLink = link;
        return Task.CompletedTask;
    }
}

public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"legajo_tests_{Guid.NewGuid():N}";

    public TestEmailService EmailService { get; } = new();

    private string ConnectionString =>
        $"Host=localhost;Port=5433;Database={_databaseName};Username=postgres;Password=postgres";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Los tests usan una base PostgreSQL real y aislada por factory.
        builder.UseSetting("Database:AutoMigrate", "false");
        builder.UseSetting("ConnectionStrings:DefaultConnection", ConnectionString);
        builder.UseSetting("Jwt:Key", "test-key-legajo-minero-32-caracteres-minimo-123456");
        builder.UseSetting("Jwt:Issuer", "LegajoMinero.Tests");
        builder.UseSetting("Jwt:Audience", "LegajoMinero.Tests");
        builder.UseSetting(
            "Security:EncryptionKey",
            "kwcgSWJPzLsBJYTQWFFb2deh+sQ0BMHxc3gzNBRZM2Q=");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IEmailService>();
            services.AddSingleton<IEmailService>(EmailService);
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                using var scope = Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureDeleted();
            }
            catch
            {
                // Si la base ya no existe, no hay nada que limpiar.
            }
        }

        base.Dispose(disposing);
    }
}
