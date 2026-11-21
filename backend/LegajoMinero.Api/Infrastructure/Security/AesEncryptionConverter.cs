using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LegajoMinero.Api.Infrastructure.Security;

/// <summary>
/// Cifra/descifra una propiedad de texto obligatoria.
/// </summary>
public sealed class AesEncryptionConverter : ValueConverter<string, string>
{
    public AesEncryptionConverter(AesEncryptionService encryption)
        : base(
            value => encryption.Encrypt(value),
            value => encryption.Decrypt(value))
    {
    }
}

/// <summary>
/// Cifra/descifra una propiedad de texto opcional, preservando los nulos.
/// </summary>
public sealed class NullableAesEncryptionConverter : ValueConverter<string?, string?>
{
    public NullableAesEncryptionConverter(AesEncryptionService encryption)
        : base(
            value => value == null ? null : encryption.Encrypt(value),
            value => value == null ? null : encryption.Decrypt(value))
    {
    }
}
