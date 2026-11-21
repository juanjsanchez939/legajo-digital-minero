using LegajoMinero.Api.Domain.Entities;
using LegajoMinero.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LegajoMinero.Api.Infrastructure.Data.Configurations;

public class DocumentoConfiguration : IEntityTypeConfiguration<Documento>
{
    public void Configure(EntityTypeBuilder<Documento> builder)
    {
        builder.ToTable("documentos");
        builder.HasKey(x => x.IdDocumento);

        builder.Property(x => x.IdDocumento).HasColumnName("id_documento");
        builder.Property(x => x.IdUsuario).HasColumnName("id_usuario").IsRequired();
        builder.Property(x => x.IdTipoDocumento).HasColumnName("id_tipo_documento").IsRequired();
        builder.Property(x => x.SubidoPorId).HasColumnName("subido_por_id").IsRequired();
        builder.Property(x => x.NombreArchivo).HasColumnName("nombre_archivo").HasMaxLength(255).IsRequired();
        builder.Property(x => x.RutaArchivo).HasColumnName("ruta_archivo").HasColumnType("text").IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasColumnType("text");
        builder.Property(x => x.FechaVencimiento).HasColumnName("fecha_vencimiento").HasColumnType("date");
        builder.Property(x => x.TieneVencimiento).HasColumnName("tiene_vencimiento").HasDefaultValue(true).IsRequired();
        builder.Property(x => x.FechaCarga).HasColumnName("fecha_carga").HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.FechaActualizacion).HasColumnName("fecha_actualizacion").HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.Estado)
            .HasColumnName("estado")
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(EstadoDocumento.VIGENTE)
            .IsRequired();
        builder.Property(x => x.EstadoRevision)
            .HasColumnName("estado_revision")
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(EstadoRevision.PENDIENTE)
            .IsRequired();
        builder.Property(x => x.ObservacionRrhh).HasColumnName("observacion_rrhh").HasColumnType("text");

        builder.HasOne(x => x.Usuario)
            .WithMany(x => x.Documentos)
            .HasForeignKey(x => x.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TipoDocumento)
            .WithMany(x => x.Documentos)
            .HasForeignKey(x => x.IdTipoDocumento)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SubidoPor)
            .WithMany(x => x.DocumentosSubidos)
            .HasForeignKey(x => x.SubidoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.IdUsuario, x.IdTipoDocumento }).IsUnique();
        builder.HasIndex(x => new { x.IdUsuario, x.Estado, x.EstadoRevision });
        builder.HasIndex(x => x.FechaVencimiento);
    }
}
