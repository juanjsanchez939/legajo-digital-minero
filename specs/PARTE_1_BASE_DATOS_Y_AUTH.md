# PARTE 1: Base de Datos, Arquitectura Backend (.NET 10) y Autenticación

> ⚠️ **REVISAR DOCUMENTACIÓN PARA IMPLEMENTACIÓN:**  
> - Revisar sección **"MODELO ENTIDAD-RELACIÓN"** y diagrama DER (`image2.png`) en [TFT_Entrega_2_Actualizado.docx](file:///C:/Users/juan-dev/Desktop/PF/TFT_Entrega_2_Actualizado.docx).  
> - Revisar **"HISTORIAS DE USUARIO (HU-01 a HU-04)"** y sección **"SEGURIDAD"** en [TFT_Entrega_2_Actualizado.docx](file:///C:/Users/juan-dev/Desktop/PF/TFT_Entrega_2_Actualizado.docx) para criterios de aceptación exactos y políticas de bloqueo/tokens.

---

## 1. Objetivo de esta Parte
Dejar creada y compilando la solución Backend en **.NET 10 (`ASP.NET Core 10 Web API`)** con **Entity Framework Core 10** y **PostgreSQL**, incluyendo las 7 tablas del DER migradas, el *Database Seeder* inicial y todo el módulo de Autenticación, Sesiones (JWT + Refresh Token) y Perfil (`HU-01`, `HU-02`, `HU-03`, `HU-04`).

---

## 2. Estructura del Proyecto Backend (`backend/LegajoMinero.Api`)

Organizar el proyecto en capas limpias:
- `Domain/Entities/`: Las 7 entidades del DER.
- `Domain/Enums/` o constantes: Roles (`TRABAJADOR`, `ADMINISTRADOR`), Estados de documento (`VIGENTE`, `PROXIMO`, `VENCIDO`), Estados de revisión (`PENDIENTE`, `VALIDADO`, `OBSERVADO`), Orígenes de alerta (`AUTOMATICA`, `MANUAL_RRHH`), Métodos de verificación (`QR`, `DNI`).
- `Infrastructure/Data/`: `AppDbContext.cs`, configuraciones Fluent API, migraciones EF Core 10 y `DbInitializer.cs` (Seeder).
- `Infrastructure/Repositories/`: Repositorios para acceso a datos.
- `Application/DTOs/` y `Application/Services/`: `AuthService`, `UsuarioService`, `SesionService`.
- `Controllers/`: `AuthController`, `UsuarioController`.

---

## 3. Especificación Exacta de las 7 Tablas (PostgreSQL + EF Core 10)

### 3.1. `usuarios` (Entidad 1)
- `id_usuario` (`int`, PK, Identity)
- `nombre_completo` (`varchar(100)`, NOT NULL)
- `dni` (`varchar(20)`, NOT NULL, **UNIQUE INDEX**)
- `email` (`varchar(100)`, NOT NULL, **UNIQUE INDEX**)
- `telefono` (`varchar(20)`, NOT NULL)
- `especialidad` (`varchar(50)`, NULL)
- `foto_perfil_url` (`text`, NULL)
- `contrasena_hash` (`varchar(255)`, NOT NULL — hash PBKDF2 con salt mediante `IPasswordHasher<Usuario>`)
- `rol` (`varchar(20)`, NOT NULL, default `"TRABAJADOR"` — valores: `"TRABAJADOR"`, `"ADMINISTRADOR"`)
- `qr_token` (`Guid` / `uuid`, NOT NULL, **UNIQUE INDEX**, generado con `Guid.NewGuid()` al crear el usuario)
- `recibir_alertas_email` (`bool`, NOT NULL, default `true`)
- `token_recuperacion_hash` (`varchar(255)`, NULL)
- `token_recuperacion_expira` (`DateTime?`, NULL)
- `intentos_fallidos` (`int`, NOT NULL, default `0`)
- `bloqueado_hasta` (`DateTime?`, NULL)
- `consentimiento_datos_fecha` (`DateTime`, NOT NULL — fecha/hora UTC en que el trabajador aceptó el tratamiento de datos personales y médicos según Ley N° 25.326 al registrarse)
- `fecha_registro` (`DateTime`, NOT NULL, default UTC now)
- `ultimo_acceso` (`DateTime?`, NULL)
- `activo` (`bool`, NOT NULL, default `true`)

### 3.2. `tipos_documento` (Entidad 3)
- `id_tipo_documento` (`int`, PK, Identity)
- `nombre` (`varchar(50)`, NOT NULL)
- `descripcion` (`varchar(255)`, NULL)
- `obligatorio` (`bool`, NOT NULL, default `true`)
- `dias_aviso` (`int`, NOT NULL, default `15`)
- `activo` (`bool`, NOT NULL, default `true`)

### 3.3. `documentos` (Entidad 2)
- `id_documento` (`int`, PK, Identity)
- `id_usuario` (`int`, FK → `usuarios.id_usuario`, NOT NULL)
- `id_tipo_documento` (`int`, FK → `tipos_documento.id_tipo_documento`, NOT NULL)
- `subido_por_id` (`int`, FK → `usuarios.id_usuario`, NOT NULL)
- `nombre_archivo` (`varchar(255)`, NOT NULL)
- `ruta_archivo` (`text`, NOT NULL)
- `descripcion` (`text`, NULL)
- `fecha_vencimiento` (`DateOnly?`, NULL)
- `tiene_vencimiento` (`bool`, NOT NULL, default `true`)
- `fecha_carga` (`DateTime`, NOT NULL)
- `fecha_actualizacion` (`DateTime`, NOT NULL)
- `estado` (`varchar(20)`, NOT NULL — `"VIGENTE"`, `"PROXIMO"`, `"VENCIDO"`)
- `estado_revision` (`varchar(20)`, NOT NULL — `"PENDIENTE"`, `"VALIDADO"`, `"OBSERVADO"`)
- `observacion_rrhh` (`text`, NULL)
- **Restricción única compuesta:** `UNIQUE (id_usuario, id_tipo_documento)` para garantizar 1 documento activo por tipo por trabajador.

### 3.4. `alertas` (Entidad 4)
- `id_alerta` (`int`, PK, Identity)
- `id_usuario` (`int`, FK → `usuarios.id_usuario`, NOT NULL)
- `id_documento` (`int?`, FK → `documentos.id_documento`, **NULLABLE** para permitir alertas manuales generales de RRHH)
- `tipo_alerta` (`varchar(20)`, NOT NULL — `"PUSH"`, `"EMAIL"`, `"DASHBOARD"`)
- `origen` (`varchar(20)`, NOT NULL — `"AUTOMATICA"`, `"MANUAL_RRHH"`)
- `dias_anticipacion` (`int?`, NULL — `15`, `7`, `3`, `0`)
- `mensaje` (`text`, NOT NULL)
- `fecha_envio` (`DateTime`, NOT NULL)
- `fecha_leida` (`DateTime?`, NULL)
- `enviada` (`bool`, NOT NULL, default `true`)

### 3.5. `verificaciones` (Entidad 5 — Auditoría de Garita y Accesos RRHH)
- `id_verificacion` (`int`, PK, Identity)
- `id_usuario` (`int`, FK → `usuarios.id_usuario`, NOT NULL — trabajador cuyo legajo/documento fue consultado)
- `verificador_usuario_id` (`int?`, FK → `usuarios.id_usuario`, **NULLABLE** — `NULL` si fue consulta pública en garita; `id_usuario` del administrador si fue acceso de RRHH al legajo o archivo médico)
- `id_documento` (`int?`, FK → `documentos.id_documento`, **NULLABLE** — `NULL` si fue escaneo QR/DNI en garita o apertura general del legajo; contiene el `id_documento` exacto cuando RRHH abre/descarga un archivo en `GET /api/documentos/{id}/archivo`)
- `metodo` (`varchar(15)`, NOT NULL — `"QR"`, `"DNI"`, `"PANEL_RRHH"`)
- `ip_verificador` (`varchar(45)`, NULL)
- `estado_verificado` (`varchar(20)`, NOT NULL — `"VERDE"`, `"AMARILLO"`, `"ROJO"`)
- `fecha_verificacion` (`DateTime`, NOT NULL)

### 3.8. Índices de Rendimiento Obligatorios (`RNF 03` < 3 segundos)
Configurar en `AppDbContext.OnModelCreating`:
- `usuarios`: índices únicos en `dni`, `email`, `qr_token` + índice en `(rol, activo)`.
- `documentos`: índice único compuesto `(id_usuario, id_tipo_documento)` + índices en `(id_usuario, estado, estado_revision)` y `(fecha_vencimiento)`.
- `alertas`: índice en `(id_usuario, fecha_leida, fecha_envio)`.
- `verificaciones`: índice en `(id_usuario, fecha_verificacion)`.

### 3.6. `sesiones` (Entidad 6)
- `id_sesion` (`int`, PK, Identity)
- `id_usuario` (`int`, FK → `usuarios.id_usuario`, NOT NULL)
- `refresh_token_hash` (`varchar(255)`, NOT NULL)
- `ip_acceso` (`varchar(45)`, NULL)
- `dispositivo` (`varchar(255)`, NULL)
- `fecha_inicio` (`DateTime`, NOT NULL)
- `fecha_expiracion` (`DateTime`, NOT NULL)
- `fecha_cierre` (`DateTime?`, NULL)
- `activa` (`bool`, NOT NULL, default `true`)

### 3.7. `backups` (Entidad 7)
- `id_backup` (`int`, PK, Identity)
- `fecha_backup` (`DateTime`, NOT NULL)
- `tamano_mb` (`decimal(10,2)`, NOT NULL)
- `ubicacion_url` (`text`, NOT NULL)
- `tipo_backup` (`varchar(20)`, NOT NULL — `"COMPLETO"`, `"INCREMENTAL"`)
- `exitoso` (`bool`, NOT NULL)

---

## 4. Database Seeder Inicial (`DbInitializer`)
Al arrancar la aplicación, si la base de datos está vacía, insertar automáticamente:
1. **Los 6 `tipos_documento` oficiales de la tesis:**
   - `1`: Examen Médico (`obligatorio = true`, `dias_aviso = 15`)
   - `2`: Carnet de Conducir Alta Montaña (`obligatorio = true`, `dias_aviso = 15`)
   - `3`: Curso de Seguridad e Higiene (`obligatorio = true`, `dias_aviso = 15`)
   - `4`: Inducción General (`obligatorio = true`, `dias_aviso = 15`)
   - `5`: Certificado de Altura (`obligatorio = false`, `dias_aviso = 15`)
   - `6`: Título Profesional (`obligatorio = false`, `dias_aviso = 30`)
2. **Cuenta inicial de RRHH (`ADMINISTRADOR`):**
   - Email: `rrhh@legajominero.com.ar` | DNI: `00000001` | Rol: `ADMINISTRADOR` | Contraseña inicial configurada en `appsettings.json` (mínimo 8 caracteres).

---

## 5. Endpoints a Implementar en la Parte 1

- `POST /api/auth/registrar` (`HU-01` + Ley N° 25.326):
  - Valida formato de email, contraseña `>= 8 caracteres`, unicidad de `dni` y `email`, y exige `aceptaConsentimientoDatos == true` (si es `false`, rechaza con HTTP 400).
  - Guarda `consentimiento_datos_fecha = DateTime.UtcNow`, fuerza `rol = "TRABAJADOR"`, genera `qr_token = Guid.NewGuid()`, hashea contraseña con PBKDF2 y devuelve JWT + Refresh Token.
- `POST /api/auth/login` (`HU-02`):
  - Verifica si `bloqueado_hasta > DateTime.UtcNow` (si está bloqueado, retorna HTTP 423/403 indicando minutos restantes).
  - Si falla la contraseña, incrementa `intentos_fallidos`; al llegar a `5`, setea `bloqueado_hasta = DateTime.UtcNow.AddMinutes(15)`.
  - Si es exitoso, resetea `intentos_fallidos = 0`, actualiza `ultimo_acceso`, crea registro en `sesiones` y retorna Access Token JWT + Refresh Token.
- `POST /api/auth/refresh`:
  - Valida el `refresh_token_hash` en `sesiones` (`activa == true` y `fecha_expiracion > UtcNow`), rota el token y emite nuevo JWT.
- `POST /api/auth/logout`:
  - Marca la sesión actual como `activa = false` y `fecha_cierre = UtcNow`.
- `POST /api/auth/solicitar-recuperacion` y `POST /api/auth/restablecer-contrasena` (`HU-03`):
  - Genera token temporal con validez de 1 hora (`token_recuperacion_hash` y `token_recuperacion_expira`) y permite cambiar la contraseña (`>= 8 caracteres`).
- `GET /api/usuarios/perfil`, `PUT /api/usuarios/perfil`, `POST /api/usuarios/foto-perfil` y `PUT /api/usuarios/cambiar-contrasena` (`HU-04`):
  - Permite editar `nombre_completo`, `especialidad`, `telefono`, `recibir_alertas_email` y foto de perfil. **El `dni` es de solo lectura**.
  - El cambio de contraseña exige validar la contraseña actual primero.
- `GET /api/sesiones/mis-sesiones` y `DELETE /api/sesiones/cerrar-remotas` (`SesionService` — Contingencia `C-13`):
  - Permite al trabajador ver sus sesiones activas (dispositivo, IP, fecha de inicio) y cerrar remotamente todas las sesiones abiertas en otros dispositivos en caso de pérdida o robo del celular.

---

## 5.1. Requisitos de Seguridad Transversales a Configurar en `Program.cs`
> ⚠️ **REVISAR DOCUMENTACIÓN PARA IMPLEMENTACIÓN:** Ver tablas de la sección **"SEGURIDAD"** en [TFT_Entrega_2_Actualizado.docx](file:///C:/Users/juan-dev/Desktop/PF/TFT_Entrega_2_Actualizado.docx).
- **Duración de Tokens:**
  - `Access Token JWT`: **30 minutos**.
  - `Refresh Token`: **7 días** (rotativo, guardado como hash en `sesiones`).
  - `Token de recuperación de contraseña`: **60 minutos (1 hora)**.
- **Rate Limiting (`AddRateLimiter`):**
  - Política `"auth"` (`/api/auth/*`): **10 peticiones / minuto por IP**.
  - Política `"verificacion"` (`/api/verificacion/*`): **30 peticiones / minuto por IP**.
  - Política global API: **120 peticiones / minuto por IP**.
- **CORS (`AddCors`):** Permitir únicamente los orígenes configurados en `appsettings.json`.
- **Cifrado Simétrico AES-256 (`AesEncryptionConverter` en EF Core):**
  - **No cifrar `dni` ni `email`** porque se requieren en texto claro indexado (`UNIQUE`) para login y para búsqueda en garita (`HU-18`).
  - **Cifrar con AES-256** en base de datos mediante el mismo `ValueConverter<string, string>` (`AesEncryptionConverter`) en `AppDbContext` las 4 columnas que pueden contener datos sensibles o motivos médicos/documentales:
    1. `usuarios.telefono` (ampliar tipo en BD a `varchar(255)` o `text` para alojar el ciphertext Base64)
    2. `documentos.descripcion`
    3. `documentos.observacion_rrhh`
    4. `alertas.mensaje`
- **IP Real detrás de Reverse Proxy Nginx (`UseForwardedHeaders`):**
  - Configurar `ForwardedHeadersOptions` (`XForwardedFor | XForwardedProto`) al inicio del pipeline en `Program.cs` para que el `RateLimiter` y los logs de `verificaciones` y `sesiones` capturen la IP real del dispositivo y no la IP interna de Nginx (`127.0.0.1`).
- **Zona Horaria Oficial:**
  - Persistir fechas y horas de auditoría en **UTC** (`DateTime.UtcNow`), y calcular `"hoy"` para vencimientos en la zona **`America/Argentina/San_Juan` (UTC-3)** (`TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/San_Juan")` o fallback `UTC-3`).
- **Proyecto de Tests Unitarios (`backend/LegajoMinero.Tests` — `RNF 10`):**
  - Crear desde la Parte 1 el proyecto `xUnit` en la solución `.sln` con tests de autenticación, bloqueo de 5 intentos y consentimiento Ley 25.326.

---

## 6. Checklist de Verificación (Definition of Done — Parte 1)
- [ ] Proyecto `.NET 10` compila sin errores ni warnings críticos.
- [ ] Migración inicial de EF Core 10 crea las 7 tablas en PostgreSQL con sus claves primarias, foráneas y alternas.
- [ ] Seeder inserta los 6 tipos de documento y el usuario `ADMINISTRADOR` inicial.
- [ ] Registro público fuerza rol `TRABAJADOR` y rechaza DNI o email duplicados.
- [ ] Login bloquea la cuenta por 15 minutos al 5° intento fallido consecutivo.
- [ ] Edición de perfil impide modificar el DNI y permite actualizar foto y preferencias de alerta.
