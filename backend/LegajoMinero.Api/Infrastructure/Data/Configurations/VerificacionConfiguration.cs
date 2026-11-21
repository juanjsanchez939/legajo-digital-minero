using LegajoMinero.Api.Domain.Entities;
using LegajoMinero.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LegajoMinero.Api.Infrastructure.Data.Configurations;

public class VerificacionConfiguration : IEntityTypeConfiguration<Verificacion>
{
    public void Configure(EntityTypeBuilder<Verificacion> builder)
    {
        builder.ToTable("verificaciones");
        builder.HasKey(x => x.IdVerificacion);

        builder.Property(x => x.IdVerificacion).HasColumnName("id_verificacion");
        builder.Property(x => x.IdUsuario).HasColumnName("id_usuario").IsRequired();
        builder.Property(x => x.VerificadorUsuarioId).HasColumnName("verificador_usuario_id");
        builder.Property(x => x.IdDocumento).HasColumnName("id_documento");
        builder.Property(x => x.Metodo)
            .HasColumnName("metodo")
            .HasConversion<string>()
            .HasMaxLength(15)
            .IsRequired();
        builder.Property(x => x.IpVerificador).HasColumnName("ip_verificador").HasMaxLength(45);
        builder.Property(x => x.EstadoVerificado)
            .HasColumnName("estado_verificado")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x => x.FechaVerificacion).HasColumnName("fecha_verificacion").HasDefaultValueSql("now()").IsRequired();

        builder.HasOne(x => x.Usuario)
            .WithMany(x => x.Verificaciones)
            .HasForeignKey(x => x.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.VerificadorUsuario)
            .WithMany(x => x.VerificacionesRealizadas)
            .HasForeignKey(x => x.VerificadorUsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Documento)
            .WithMany(x => x.Verificaciones)
            .HasForeignKey(x => x.IdDocumento)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.IdUsuario, x.FechaVerificacion });
    }
}
