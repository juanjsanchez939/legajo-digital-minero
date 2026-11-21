using LegajoMinero.Api.Infrastructure.Security;
using Microsoft.Extensions.Configuration;

namespace LegajoMinero.Tests;

public class AesEncryptionTests
{
    private const string DevKey = "kwcgSWJPzLsBJYTQWFFb2deh+sQ0BMHxc3gzNBRZM2Q=";

    private static AesEncryptionService CreateService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Security:EncryptionKey"] = DevKey
            })
            .Build();

        return new AesEncryptionService(configuration);
    }

    [Fact]
    public void EncryptThenDecrypt_ReturnsOriginal()
    {
        var service = CreateService();
        const string original = "Obra social 123456 / observación médica";

        var encrypted = service.Encrypt(original);

        Assert.NotEqual(original, encrypted);
        Assert.StartsWith("v1:", encrypted);
        Assert.Equal(original, service.Decrypt(encrypted));
    }

    [Fact]
    public void Encrypt_SameInput_ProducesDifferentCiphertexts()
    {
        var service = CreateService();

        var first = service.Encrypt("1234567890");
        var second = service.Encrypt("1234567890");

        Assert.NotEqual(first, second);
        Assert.Equal(service.Decrypt(first), service.Decrypt(second));
    }

    [Fact]
    public void Decrypt_Plaintext_ReturnsSameValue()
    {
        var service = CreateService();

        Assert.Equal("sin cifrar", service.Decrypt("sin cifrar"));
    }
}
