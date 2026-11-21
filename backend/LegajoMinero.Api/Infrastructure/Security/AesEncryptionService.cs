using System.Security.Cryptography;
using System.Text;

namespace LegajoMinero.Api.Infrastructure.Security;

/// <summary>
/// Cifrado simétrico AES-256-GCM para columnas sensibles (teléfono, notas y
/// motivos médicos). El formato almacenado es <c>v1:{base64(nonce|cipher|tag)}</c>.
/// </summary>
public sealed class AesEncryptionService
{
    private const string Prefix = "v1:";
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private readonly byte[] _key;

    public AesEncryptionService(IConfiguration configuration)
    {
        var raw = configuration["Security:EncryptionKey"]
            ?? throw new InvalidOperationException(
                "Falta la configuración Security:EncryptionKey.");

        _key = DecodeKey(raw);
        if (_key.Length != 32)
        {
            throw new InvalidOperationException(
                "Security:EncryptionKey debe representar 32 bytes (AES-256).");
        }
    }

    public string Encrypt(string plainText)
    {
        ArgumentNullException.ThrowIfNull(plainText);

        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipher = new byte[plainBytes.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plainBytes, cipher, tag);

        var payload = new byte[NonceSize + cipher.Length + TagSize];
        Buffer.BlockCopy(nonce, 0, payload, 0, NonceSize);
        Buffer.BlockCopy(cipher, 0, payload, NonceSize, cipher.Length);
        Buffer.BlockCopy(tag, 0, payload, NonceSize + cipher.Length, TagSize);

        return Prefix + Convert.ToBase64String(payload);
    }

    public string Decrypt(string encrypted)
    {
        ArgumentNullException.ThrowIfNull(encrypted);

        // Permite leer datos previos en claro si alguna migración los dejó así.
        if (!encrypted.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return encrypted;
        }

        var payload = Convert.FromBase64String(encrypted[Prefix.Length..]);
        if (payload.Length < NonceSize + TagSize)
        {
            throw new CryptographicException("El valor cifrado tiene un formato inválido.");
        }

        var nonce = payload.AsSpan(0, NonceSize);
        var tag = payload.AsSpan(payload.Length - TagSize, TagSize);
        var cipher = payload.AsSpan(NonceSize, payload.Length - NonceSize - TagSize);
        var plain = new byte[cipher.Length];

        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(nonce, cipher, tag, plain);

        return Encoding.UTF8.GetString(plain);
    }

    private static byte[] DecodeKey(string raw)
    {
        try
        {
            return Convert.FromBase64String(raw);
        }
        catch (FormatException)
        {
            // Comodidad para desarrollo: cualquier frase se deriva a 32 bytes.
            return SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        }
    }
}
