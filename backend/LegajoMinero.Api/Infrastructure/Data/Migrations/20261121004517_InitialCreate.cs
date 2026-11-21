using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LegajoMinero.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "backups",
                columns: table => new
                {
                    id_backup = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha_backup = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    tamano_mb = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    ubicacion_url = table.Column<string>(type: "text", nullable: false),
                    tipo_backup = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "COMPLETO"),
                    exitoso = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_backups", x => x.id_backup);
                });

            migrationBuilder.CreateTable(
                name: "tipos_documento",
                columns: table => new
                {
                    id_tipo_documento = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    obligatorio = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    dias_aviso = table.Column<int>(type: "integer", nullable: false, defaultValue: 15),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipos_documento", x => x.id_tipo_documento);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_completo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    dni = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    telefono = table.Column<string>(type: "text", nullable: false),
                    especialidad = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    foto_perfil_url = table.Column<string>(type: "text", nullable: true),
                    contrasena_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    rol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "TRABAJADOR"),
                    qr_token = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    recibir_alertas_email = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    token_recuperacion_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    token_recuperacion_expira = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    intentos_fallidos = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    bloqueado_hasta = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ultimo_acceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    consentimiento_datos_fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id_usuario);
                });

            migrationBuilder.CreateTable(
                name: "documentos",
                columns: table => new
                {
                    id_documento = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    id_tipo_documento = table.Column<int>(type: "integer", nullable: false),
                    subido_por_id = table.Column<int>(type: "integer", nullable: false),
                    nombre_archivo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ruta_archivo = table.Column<string>(type: "text", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    fecha_vencimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    tiene_vencimiento = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    fecha_carga = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    fecha_actualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "VIGENTE"),
                    estado_revision = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "PENDIENTE"),
                    observacion_rrhh = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documentos", x => x.id_documento);
                    table.ForeignKey(
                        name: "FK_documentos_tipos_documento_id_tipo_documento",
                        column: x => x.id_tipo_documento,
                        principalTable: "tipos_documento",
                        principalColumn: "id_tipo_documento",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_documentos_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_documentos_usuarios_subido_por_id",
                        column: x => x.subido_por_id,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sesiones",
                columns: table => new
                {
                    id_sesion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    refresh_token_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ip_acceso = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    dispositivo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    fecha_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    fecha_expiracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_cierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    activa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sesiones", x => x.id_sesion);
                    table.ForeignKey(
                        name: "FK_sesiones_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "alertas",
                columns: table => new
                {
                    id_alerta = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    id_documento = table.Column<int>(type: "integer", nullable: true),
                    tipo_alerta = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "DASHBOARD"),
                    origen = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "AUTOMATICA"),
                    dias_anticipacion = table.Column<int>(type: "integer", nullable: true),
                    mensaje = table.Column<string>(type: "text", nullable: false),
                    fecha_envio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    fecha_leida = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    enviada = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alertas", x => x.id_alerta);
                    table.ForeignKey(
                        name: "FK_alertas_documentos_id_documento",
                        column: x => x.id_documento,
                        principalTable: "documentos",
                        principalColumn: "id_documento",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_alertas_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "verificaciones",
                columns: table => new
                {
                    id_verificacion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    verificador_usuario_id = table.Column<int>(type: "integer", nullable: true),
                    id_documento = table.Column<int>(type: "integer", nullable: true),
                    metodo = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    ip_verificador = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    estado_verificado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_verificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_verificaciones", x => x.id_verificacion);
                    table.ForeignKey(
                        name: "FK_verificaciones_documentos_id_documento",
                        column: x => x.id_documento,
                        principalTable: "documentos",
                        principalColumn: "id_documento",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_verificaciones_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_verificaciones_usuarios_verificador_usuario_id",
                        column: x => x.verificador_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_alertas_id_documento",
                table: "alertas",
                column: "id_documento");

            migrationBuilder.CreateIndex(
                name: "IX_alertas_id_usuario_fecha_leida_fecha_envio",
                table: "alertas",
                columns: new[] { "id_usuario", "fecha_leida", "fecha_envio" });

            migrationBuilder.CreateIndex(
                name: "IX_documentos_fecha_vencimiento",
                table: "documentos",
                column: "fecha_vencimiento");

            migrationBuilder.CreateIndex(
                name: "IX_documentos_id_tipo_documento",
                table: "documentos",
                column: "id_tipo_documento");

            migrationBuilder.CreateIndex(
                name: "IX_documentos_id_usuario_estado_estado_revision",
                table: "documentos",
                columns: new[] { "id_usuario", "estado", "estado_revision" });

            migrationBuilder.CreateIndex(
                name: "IX_documentos_id_usuario_id_tipo_documento",
                table: "documentos",
                columns: new[] { "id_usuario", "id_tipo_documento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_documentos_subido_por_id",
                table: "documentos",
                column: "subido_por_id");

            migrationBuilder.CreateIndex(
                name: "IX_sesiones_id_usuario_activa",
                table: "sesiones",
                columns: new[] { "id_usuario", "activa" });

            migrationBuilder.CreateIndex(
                name: "IX_sesiones_refresh_token_hash",
                table: "sesiones",
                column: "refresh_token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tipos_documento_nombre",
                table: "tipos_documento",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_dni",
                table: "usuarios",
                column: "dni",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_email",
                table: "usuarios",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_qr_token",
                table: "usuarios",
                column: "qr_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_rol_activo",
                table: "usuarios",
                columns: new[] { "rol", "activo" });

            migrationBuilder.CreateIndex(
                name: "IX_verificaciones_id_documento",
                table: "verificaciones",
                column: "id_documento");

            migrationBuilder.CreateIndex(
                name: "IX_verificaciones_id_usuario_fecha_verificacion",
                table: "verificaciones",
                columns: new[] { "id_usuario", "fecha_verificacion" });

            migrationBuilder.CreateIndex(
                name: "IX_verificaciones_verificador_usuario_id",
                table: "verificaciones",
                column: "verificador_usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alertas");

            migrationBuilder.DropTable(
                name: "backups");

            migrationBuilder.DropTable(
                name: "sesiones");

            migrationBuilder.DropTable(
                name: "verificaciones");

            migrationBuilder.DropTable(
                name: "documentos");

            migrationBuilder.DropTable(
                name: "tipos_documento");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
