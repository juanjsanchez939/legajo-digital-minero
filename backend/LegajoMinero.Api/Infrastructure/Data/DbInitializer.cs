using LegajoMinero.Api.Domain.Entities;
using LegajoMinero.Api.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LegajoMinero.Api.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(DbInitializer));

        await db.Database.MigrateAsync();

        if (!await db.TiposDocumento.AnyAsync())
        {
            db.TiposDocumento.AddRange(
                new TipoDocumento { Nombre = "Examen Médico", Obligatorio = true, DiasAviso = 15 },
                new TipoDocumento { Nombre = "Carnet de Conducir Alta Montaña", Obligatorio = true, DiasAviso = 15 },
                new TipoDocumento { Nombre = "Curso de Seguridad e Higiene", Obligatorio = true, DiasAviso = 15 },
                new TipoDocumento { Nombre = "Inducción General", Obligatorio = true, DiasAviso = 15 },
                new TipoDocumento { Nombre = "Certificado de Altura", Obligatorio = false, DiasAviso = 15 },
                new TipoDocumento { Nombre = "Título Profesional", Obligatorio = false, DiasAviso = 30 });

            await db.SaveChangesAsync();
            logger.LogInformation("Seeder: se crearon los 6 tipos de documento.");
        }

        var adminEmail = configuration["Seed:AdminEmail"] ?? "rrhh@legajominero.com.ar";
        var adminDni = configuration["Seed:AdminDni"] ?? "00000001";

        if (await db.Usuarios.AnyAsync(u => u.Email == adminEmail || u.Dni == adminDni))
        {
            return;
        }

        var password = configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            throw new InvalidOperationException(
                "Seed:AdminPassword debe estar configurada con al menos 8 caracteres.");
        }

        var admin = new Usuario
        {
            NombreCompleto = "Recursos Humanos",
            Dni = adminDni,
            Email = adminEmail,
            Telefono = "0000000000",
            Especialidad = "Recursos Humanos",
            Rol = RolUsuario.ADMINISTRADOR,
            QrToken = Guid.NewGuid(),
            FechaRegistro = DateTime.UtcNow,
            ConsentimientoDatosFecha = DateTime.UtcNow,
            Activo = true
        };

        admin.ContrasenaHash = hasher.HashPassword(admin, password);
        db.Usuarios.Add(admin);
        await db.SaveChangesAsync();

        logger.LogInformation("Seeder: se creó el administrador {Email}.", adminEmail);
    }
}
