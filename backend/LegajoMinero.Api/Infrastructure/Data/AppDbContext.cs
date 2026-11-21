using LegajoMinero.Api.Domain.Entities;
using LegajoMinero.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace LegajoMinero.Api.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly AesEncryptionConverter _encryptionConverter;
    private readonly NullableAesEncryptionConverter _nullableEncryptionConverter;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        AesEncryptionService encryption)
        : base(options)
    {
        _encryptionConverter = new AesEncryptionConverter(encryption);
        _nullableEncryptionConverter = new NullableAesEncryptionConverter(encryption);
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<TipoDocumento> TiposDocumento => Set<TipoDocumento>();
    public DbSet<Documento> Documentos => Set<Documento>();
    public DbSet<Alerta> Alertas => Set<Alerta>();
    public DbSet<Verificacion> Verificaciones => Set<Verificacion>();
    public DbSet<Sesion> Sesiones => Set<Sesion>();
    public DbSet<Backup> Backups => Set<Backup>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Cifrado transparente de las columnas con datos sensibles.
        modelBuilder.Entity<Usuario>()
            .Property(x => x.Telefono)
            .HasConversion(_encryptionConverter);

        modelBuilder.Entity<Documento>()
            .Property(x => x.Descripcion)
            .HasConversion(_nullableEncryptionConverter);

        modelBuilder.Entity<Documento>()
            .Property(x => x.ObservacionRrhh)
            .HasConversion(_nullableEncryptionConverter);

        modelBuilder.Entity<Alerta>()
            .Property(x => x.Mensaje)
            .HasConversion(_encryptionConverter);
    }
}
