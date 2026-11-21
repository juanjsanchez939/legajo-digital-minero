using LegajoMinero.Api.Domain.Entities;
using LegajoMinero.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LegajoMinero.Api.Infrastructure.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");
        builder.HasKey(x => x.IdUsuario);

        builder.Property(x => x.IdUsuario).HasColumnName("id_usuario");
        builder.Property(x => x.NombreCompleto).HasColumnName("nombre_completo").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Dni).HasColumnName("dni").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Telefono).HasColumnName("telefono").HasColumnType("text").IsRequired();
        builder.Property(x => x.Especialidad).HasColumnName("especialidad").HasMaxLength(50);
        builder.Property(x => x.FotoPerfilUrl).HasColumnName("foto_perfil_url").HasColumnType("text");
        builder.Property(x => x.ContrasenaHash).HasColumnName("contrasena_hash").HasMaxLength(255).IsRequired();
        builder.Property(x => x.Rol)
            .HasColumnName("rol")
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(RolUsuario.TRABAJADOR)
            .IsRequired();
        builder.Property(x => x.QrToken)
            .HasColumnName("qr_token")
            .HasDefaultValueSql("gen_random_uuid()")
            .IsRequired();
        builder.Property(x => x.RecibirAlertasEmail).HasColumnName("recibir_alertas_email").HasDefaultValue(true).IsRequired();
        builder.Property(x => x.TokenRecuperacionHash).HasColumnName("token_recuperacion_hash").HasMaxLength(255);
        builder.Property(x => x.TokenRecuperacionExpira).HasColumnName("token_recuperacion_expira");
        builder.Property(x => x.IntentosFallidos).HasColumnName("intentos_fallidos").HasDefaultValue(0).IsRequired();
        builder.Property(x => x.BloqueadoHasta).HasColumnName("bloqueado_hasta");
        builder.Property(x => x.FechaRegistro).HasColumnName("fecha_registro").HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.UltimoAcceso).HasColumnName("ultimo_acceso");
        builder.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true).IsRequired();
        builder.Property(x => x.ConsentimientoDatosFecha)
            .HasColumnName("consentimiento_datos_fecha")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.HasIndex(x => x.Dni).IsUnique();
        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.QrToken).IsUnique();
        builder.HasIndex(x => new { x.Rol, x.Activo });
    }
}
