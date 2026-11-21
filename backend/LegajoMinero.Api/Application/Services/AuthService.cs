using LegajoMinero.Api.Application.DTOs;
using LegajoMinero.Api.Application.Interfaces;
using LegajoMinero.Api.Domain.Entities;
using LegajoMinero.Api.Domain.Enums;
using LegajoMinero.Api.Infrastructure.Data;
using LegajoMinero.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LegajoMinero.Api.Application.Services;

public sealed class AuthService
{
    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 15;

    private readonly AppDbContext _db;
    private readonly IPasswordHasher<Usuario> _hasher;
    private readonly JwtTokenService _jwt;
    private readonly IEmailService _email;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext db,
        IPasswordHasher<Usuario> hasher,
        JwtTokenService jwt,
        IEmailService email,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _db = db;
        _hasher = hasher;
        _jwt = jwt;
        _email = email;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResult> RegisterAsync(
        RegisterRequest request,
        string? ip,
        string? device,
        CancellationToken cancellationToken = default)
    {
        if (!request.AceptaConsentimientoDatos)
        {
            return AuthResult.Fail(
                400,
                "Debés aceptar el tratamiento de datos personales y médicos (Ley N° 25.326).",
                "Consentimiento requerido");
        }

        var email = NormalizeEmail(request.Email);
        var dni = request.Dni.Trim();

        if (await _db.Usuarios.AnyAsync(u => u.Email == email || u.Dni == dni, cancellationToken))
        {
            return AuthResult.Fail(
                409,
                "Ya existe una cuenta con ese DNI o correo electrónico.",
                "Datos duplicados");
        }

        var usuario = new Usuario
        {
            NombreCompleto = request.NombreCompleto.Trim(),
            Dni = dni,
            Email = email,
            Telefono = request.Telefono.Trim(),
            Especialidad = string.IsNullOrWhiteSpace(request.Especialidad)
                ? null
                : request.Especialidad.Trim(),
            Rol = RolUsuario.TRABAJADOR,
            QrToken = Guid.NewGuid(),
            FechaRegistro = DateTime.UtcNow,
            ConsentimientoDatosFecha = DateTime.UtcNow,
            Activo = true
        };

        usuario.ContrasenaHash = _hasher.HashPassword(usuario, request.Contrasena);
        _db.Usuarios.Add(usuario);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return AuthResult.Fail(
                409,
                "Ya existe una cuenta con ese DNI o correo electrónico.",
                "Datos duplicados");
        }

        var (sesion, refreshToken) = await CreateSessionAsync(usuario, ip, device, cancellationToken);
        _logger.LogInformation("Usuario registrado: {Email}", usuario.Email);

        return AuthResult.Ok(BuildResponse(usuario, sesion, refreshToken), 201);
    }

    public async Task<AuthResult> LoginAsync(
        LoginRequest request,
        string? ip,
        string? device,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (usuario is null)
        {
            return AuthResult.Fail(401, "Correo o contraseña incorrectos.", "No autorizado");
        }

        var now = DateTime.UtcNow;

        if (usuario.BloqueadoHasta is { } blockedUntil && blockedUntil > now)
        {
            var minutes = Math.Max(1, (int)Math.Ceiling((blockedUntil - now).TotalMinutes));
            return AuthResult.Fail(
                423,
                $"La cuenta está bloqueada. Intentá de nuevo en {minutes} minuto(s).",
                "Cuenta bloqueada",
                minutes * 60);
        }

        if (!usuario.Activo)
        {
            return AuthResult.Fail(403, "TRABAJADOR INACTIVO / NO HABILITADO.", "Cuenta inactiva");
        }

        var verification = _hasher.VerifyHashedPassword(
            usuario,
            usuario.ContrasenaHash,
            request.Contrasena);

        if (verification == PasswordVerificationResult.Failed)
        {
            usuario.IntentosFallidos++;

            if (usuario.IntentosFallidos >= MaxFailedAttempts)
            {
                usuario.IntentosFallidos = 0;
                usuario.BloqueadoHasta = now.AddMinutes(LockoutMinutes);
                await _db.SaveChangesAsync(cancellationToken);
                _logger.LogWarning("Cuenta bloqueada por intentos fallidos: {Email}", usuario.Email);

                return AuthResult.Fail(
                    423,
                    $"La cuenta quedó bloqueada por {LockoutMinutes} minutos.",
                    "Cuenta bloqueada",
                    LockoutMinutes * 60);
            }

            await _db.SaveChangesAsync(cancellationToken);
            var remaining = MaxFailedAttempts - usuario.IntentosFallidos;

            return AuthResult.Fail(
                401,
                $"Correo o contraseña incorrectos. Te quedan {remaining} intento(s).",
                "No autorizado");
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            usuario.ContrasenaHash = _hasher.HashPassword(usuario, request.Contrasena);
        }

        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHasta = null;
        usuario.UltimoAcceso = now;

        var (sesion, refreshToken) = await CreateSessionAsync(usuario, ip, device, cancellationToken);
        _logger.LogInformation("Inicio de sesión: {Email}", usuario.Email);

        return AuthResult.Ok(BuildResponse(usuario, sesion, refreshToken));
    }

    public async Task<AuthResult> RefreshAsync(
        RefreshRequest request,
        string? ip,
        string? device,
        CancellationToken cancellationToken = default)
    {
        var hash = RefreshTokenGenerator.Hash(request.RefreshToken);
        var sesion = await _db.Sesiones
            .Include(s => s.Usuario)
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == hash, cancellationToken);

        if (sesion is null || !sesion.Activa || sesion.FechaExpiracion <= DateTime.UtcNow)
        {
            return AuthResult.Fail(
                401,
                "Sesión inválida o expirada. Iniciá sesión nuevamente.",
                "No autorizado");
        }

        if (!sesion.Usuario.Activo)
        {
            return AuthResult.Fail(403, "TRABAJADOR INACTIVO / NO HABILITADO.", "Cuenta inactiva");
        }

        sesion.Activa = false;
        sesion.FechaCierre = DateTime.UtcNow;

        var (newSession, refreshToken) = await CreateSessionAsync(
            sesion.Usuario,
            ip,
            device,
            cancellationToken);

        return AuthResult.Ok(BuildResponse(sesion.Usuario, newSession, refreshToken));
    }

    public async Task<OperationResult> LogoutAsync(
        LogoutRequest request,
        CancellationToken cancellationToken = default)
    {
        var hash = RefreshTokenGenerator.Hash(request.RefreshToken);
        var sesion = await _db.Sesiones
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == hash && s.Activa, cancellationToken);

        if (sesion is not null)
        {
            sesion.Activa = false;
            sesion.FechaCierre = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }

        return new OperationResult(204);
    }

    public async Task RequestPasswordResetAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email && u.Activo, cancellationToken);

        if (usuario is null)
        {
            // No revelar si el correo existe.
            return;
        }

        var token = RefreshTokenGenerator.Generate();
        usuario.TokenRecuperacionHash = RefreshTokenGenerator.Hash(token);
        usuario.TokenRecuperacionExpira = DateTime.UtcNow.AddHours(1);
        await _db.SaveChangesAsync(cancellationToken);

        var baseUrl = (_configuration["Frontend:BaseUrl"] ?? "http://localhost:5173").TrimEnd('/');
        var link = $"{baseUrl}/restablecer?token={Uri.EscapeDataString(token)}";
        await _email.SendPasswordResetAsync(usuario.Email, link, cancellationToken);

        _logger.LogInformation("Solicitud de recuperación para {Email}", usuario.Email);
    }

    public async Task<OperationResult> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var hash = RefreshTokenGenerator.Hash(request.Token);
        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.TokenRecuperacionHash == hash, cancellationToken);

        if (usuario is null
            || usuario.TokenRecuperacionExpira is null
            || usuario.TokenRecuperacionExpira <= DateTime.UtcNow)
        {
            return new OperationResult(
                400,
                "Token inválido",
                "El enlace de recuperación es inválido o expiró.");
        }

        usuario.ContrasenaHash = _hasher.HashPassword(usuario, request.NuevaContrasena);
        usuario.TokenRecuperacionHash = null;
        usuario.TokenRecuperacionExpira = null;
        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHasta = null;

        var sesiones = await _db.Sesiones
            .Where(s => s.IdUsuario == usuario.IdUsuario && s.Activa)
            .ToListAsync(cancellationToken);

        foreach (var sesion in sesiones)
        {
            sesion.Activa = false;
            sesion.FechaCierre = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Contraseña restablecida para {Email}", usuario.Email);

        return new OperationResult(204);
    }

    private async Task<(Sesion Sesion, string RefreshToken)> CreateSessionAsync(
        Usuario usuario,
        string? ip,
        string? device,
        CancellationToken cancellationToken)
    {
        var refreshToken = RefreshTokenGenerator.Generate();
        var days = _configuration.GetValue("Jwt:RefreshTokenDays", 7);

        var sesion = new Sesion
        {
            IdUsuario = usuario.IdUsuario,
            RefreshTokenHash = RefreshTokenGenerator.Hash(refreshToken),
            IpAcceso = ip,
            Dispositivo = device,
            FechaInicio = DateTime.UtcNow,
            FechaExpiracion = DateTime.UtcNow.AddDays(days),
            Activa = true
        };

        _db.Sesiones.Add(sesion);
        await _db.SaveChangesAsync(cancellationToken);

        return (sesion, refreshToken);
    }

    private AuthResponse BuildResponse(Usuario usuario, Sesion sesion, string refreshToken)
    {
        var (accessToken, expiresAt) = _jwt.Create(usuario, sesion.IdSesion);

        return new AuthResponse(
            accessToken,
            refreshToken,
            expiresAt,
            new UsuarioResponse(
                usuario.IdUsuario,
                usuario.NombreCompleto,
                usuario.Dni,
                usuario.Email,
                usuario.Rol.ToString(),
                usuario.QrToken,
                usuario.FotoPerfilUrl,
                usuario.Activo));
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
