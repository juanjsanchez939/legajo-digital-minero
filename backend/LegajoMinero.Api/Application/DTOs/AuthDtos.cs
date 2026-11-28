using System.ComponentModel.DataAnnotations;

namespace LegajoMinero.Api.Application.DTOs;

public sealed class RegisterRequest
{
    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El DNI es obligatorio.")]
    [RegularExpression(@"^\d{7,8}$", ErrorMessage = "El DNI debe tener 7 u 8 dígitos.")]
    public string Dni { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^[0-9+\-\s()]{6,20}$", ErrorMessage = "El teléfono no tiene un formato válido.")]
    public string Telefono { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Especialidad { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Contrasena { get; set; } = string.Empty;

    public bool AceptaConsentimientoDatos { get; set; }
}

public sealed class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Contrasena { get; set; } = string.Empty;
}

public sealed class RefreshRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public sealed class LogoutRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public sealed class ForgotPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public sealed class ResetPasswordRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required, MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string NuevaContrasena { get; set; } = string.Empty;
}

public sealed record UsuarioResponse(
    int IdUsuario,
    string NombreCompleto,
    string Dni,
    string Email,
    string Rol,
    Guid QrToken,
    string? FotoPerfilUrl,
    bool Activo);

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiraEnUtc,
    UsuarioResponse Usuario);
