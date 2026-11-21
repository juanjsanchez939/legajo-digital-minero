using LegajoMinero.Api.Domain.Entities;
using LegajoMinero.Api.Infrastructure.Data;
using LegajoMinero.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LegajoMinero.Tests;

public class ModelMetadataTests
{
    private const string DevKey = "kwcgSWJPzLsBJYTQWFFb2deh+sQ0BMHxc3gzNBRZM2Q=";

    private static AppDbContext CreateContext()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Security:EncryptionKey"] = DevKey
            })
            .Build();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=5433;Database=legajo_minero;Username=postgres;Password=postgres")
            .Options;

        return new AppDbContext(options, new AesEncryptionService(configuration));
    }

    [Fact]
    public void Model_HasTheSevenOfficialTables()
    {
        using var db = CreateContext();

        var tables = db.Model.GetEntityTypes()
            .Select(entity => entity.GetTableName())
            .OrderBy(name => name)
            .ToArray();

        Assert.Equal(
            new[]
            {
                "alertas",
                "backups",
                "documentos",
                "sesiones",
                "tipos_documento",
                "usuarios",
                "verificaciones"
            },
            tables);
    }

    [Fact]
    public void Usuario_HasUniqueDniEmailAndQrToken()
    {
        using var db = CreateContext();

        var entity = db.Model.FindEntityType(typeof(Usuario))!;
        var uniqueIndexes = entity.GetIndexes()
            .Where(index => index.IsUnique)
            .Select(index => string.Join(",", index.Properties.Select(property => property.Name)))
            .ToHashSet();

        Assert.Contains("Dni", uniqueIndexes);
        Assert.Contains("Email", uniqueIndexes);
        Assert.Contains("QrToken", uniqueIndexes);
    }

    [Fact]
    public void Documento_HasCompositeUniqueIndex()
    {
        using var db = CreateContext();

        var entity = db.Model.FindEntityType(typeof(Documento))!;
        var uniqueIndex = Assert.Single(entity.GetIndexes(), index => index.IsUnique);

        Assert.Equal(
            new[] { "IdUsuario", "IdTipoDocumento" },
            uniqueIndex.Properties.Select(property => property.Name).ToArray());
    }
}
