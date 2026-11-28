using System.Net;
using System.Net.Http.Json;
using LegajoMinero.Api.Application.DTOs;

namespace LegajoMinero.Tests;

public class AuthFlowTests
{
    private static RegisterRequest NewRegister(string? email = null, string? dni = null) => new()
    {
        NombreCompleto = "Juan Pérez",
        Dni = dni ?? Random.Shared.Next(10_000_000, 99_999_999).ToString(),
        Email = email ?? $"user-{Guid.NewGuid():N}@test.com",
        Telefono = "2641234567",
        Especialidad = "Operador",
        Contrasena = "ClaveSegura123",
        AceptaConsentimientoDatos = true
    };

    [Fact]
    public async Task Register_ReturnsTokensAndTrabajadorRole()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        var request = NewRegister();

        var response = await client.PostAsJsonAsync("/api/auth/registrar", request);
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(response.StatusCode == HttpStatusCode.Created, body);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrWhiteSpace(auth!.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(auth.RefreshToken));
        Assert.Equal("TRABAJADOR", auth.Usuario.Rol);
        Assert.Equal(request.Email.ToLowerInvariant(), auth.Usuario.Email);
        Assert.NotEqual(Guid.Empty, auth.Usuario.QrToken);
    }

    [Fact]
    public async Task Register_WithoutConsent_ReturnsBadRequest()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        var request = NewRegister();
        request.AceptaConsentimientoDatos = false;

        var response = await client.PostAsJsonAsync("/api/auth/registrar", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateDniOrEmail_ReturnsConflict()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        var request = NewRegister();
        await client.PostAsJsonAsync("/api/auth/registrar", request);

        var duplicate = NewRegister(email: request.Email, dni: request.Dni);
        var response = await client.PostAsJsonAsync("/api/auth/registrar", duplicate);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_AfterFiveFailures_BlocksAccount()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        var register = NewRegister();
        await client.PostAsJsonAsync("/api/auth/registrar", register);

        var wrong = new LoginRequest
        {
            Email = register.Email,
            Contrasena = "OtraClave123"
        };

        HttpStatusCode lastStatus = HttpStatusCode.OK;
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var response = await client.PostAsJsonAsync("/api/auth/login", wrong);
            lastStatus = response.StatusCode;
        }

        Assert.Equal(HttpStatusCode.Locked, lastStatus);

        var correct = new LoginRequest
        {
            Email = register.Email,
            Contrasena = register.Contrasena
        };

        var blocked = await client.PostAsJsonAsync("/api/auth/login", correct);
        Assert.Equal(HttpStatusCode.Locked, blocked.StatusCode);
    }

    [Fact]
    public async Task Refresh_RotatesTokenAndInvalidatesOldOne()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        var register = NewRegister();

        var registerResponse = await client.PostAsJsonAsync("/api/auth/registrar", register);
        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        var refreshResponse = await client.PostAsJsonAsync(
            "/api/auth/refresh",
            new RefreshRequest { RefreshToken = auth!.RefreshToken });

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        var refreshed = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(refreshed);
        Assert.NotEqual(auth.RefreshToken, refreshed!.RefreshToken);

        var reuse = await client.PostAsJsonAsync(
            "/api/auth/refresh",
            new RefreshRequest { RefreshToken = auth.RefreshToken });

        Assert.Equal(HttpStatusCode.Unauthorized, reuse.StatusCode);
    }

    [Fact]
    public async Task Logout_InvalidatesRefreshToken()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        var register = NewRegister();

        var registerResponse = await client.PostAsJsonAsync("/api/auth/registrar", register);
        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        var logout = await client.PostAsJsonAsync(
            "/api/auth/logout",
            new LogoutRequest { RefreshToken = auth!.RefreshToken });

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        var refresh = await client.PostAsJsonAsync(
            "/api/auth/refresh",
            new RefreshRequest { RefreshToken = auth.RefreshToken });

        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }

    [Fact]
    public async Task PasswordReset_CompletesFlowAndAllowsNewPassword()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        var register = NewRegister();
        await client.PostAsJsonAsync("/api/auth/registrar", register);

        var forgot = await client.PostAsJsonAsync(
            "/api/auth/solicitar-recuperacion",
            new ForgotPasswordRequest { Email = register.Email });

        Assert.Equal(HttpStatusCode.Accepted, forgot.StatusCode);
        Assert.NotNull(factory.EmailService.LastLink);

        var rawToken = factory.EmailService.LastLink!
            .Split("token=", StringSplitOptions.RemoveEmptyEntries)[1];
        var token = Uri.UnescapeDataString(rawToken);

        var reset = await client.PostAsJsonAsync(
            "/api/auth/restablecer-contrasena",
            new ResetPasswordRequest
            {
                Token = token,
                NuevaContrasena = "NuevaClave123"
            });

        Assert.Equal(HttpStatusCode.NoContent, reset.StatusCode);

        var login = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest
            {
                Email = register.Email,
                Contrasena = "NuevaClave123"
            });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }
}
