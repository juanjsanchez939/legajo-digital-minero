# PARTE 3: Módulo Backend de RRHH y Motor de Alertas

> ⚠️ **REVISAR DOCUMENTACIÓN PARA IMPLEMENTACIÓN:**  
> - Revisar **"HISTORIAS DE USUARIO (HU-08, HU-15, HU-16, HU-17)"** en [TFT_Entrega_2_Actualizado.docx](file:///C:/Users/juan-dev/Desktop/PF/TFT_Entrega_2_Actualizado.docx).  
> - Revisar **"Componentes del Backend (Servicios)"** y **"Flujo de Datos (Paso 7 y Paso 9)"** en [TFT_Entrega_2_Actualizado.docx](file:///C:/Users/juan-dev/Desktop/PF/TFT_Entrega_2_Actualizado.docx).

---

## 1. Objetivo de esta Parte
Completar el backend (.NET 10) implementando todas las capacidades del rol **Administrador (RRHH)** (`HU-15`, `HU-16`, `HU-17`), el centro de alertas del trabajador, el **`AlertaBackgroundService`** diario e idempotente (`HU-08`) y el **`BackupService`**.

---

## 2. Endpoints del Panel de RRHH (`AdminController` — Requiere `[Authorize(Roles = "ADMINISTRADOR")]`)

### 2.1. Monitoreo de Padrón, Baja Laboral, Creación de Admins y Exportación (`HU-15`)
- `GET /api/admin/trabajadores`:
  - Soporta paginación y filtros query string: `?page=1&pageSize=25`, `?estadoSemaforo=VERDE|AMARILLO|ROJO`, `?busqueda=dni_o_nombre`, `?activo=true|false` (optimizado con índices compuestos para responder en `< 3 segundos` aún con 500+ trabajadores — `RNF 03`).
  - Devuelve el listado paginado de trabajadores con su semáforo calculado en tiempo real, resumen de contadores (`vigentes`, `proximos`, `vencidos`, `pendientes`, `observados`, `faltantes`), `ultimoAcceso` y estado `activo`, además de métricas globales del padrón (`totalVerde`, `totalAmarillo`, `totalRojo`, `totalInactivos`).
- `POST /api/admin/usuarios/crear-admin` y `PATCH /api/admin/trabajadores/{idUsuario}/rol`:
  - Permite a un `ADMINISTRADOR` autenticado dar de alta una nueva cuenta con `rol = "ADMINISTRADOR"` o promover/revocar el rol de administrador a un usuario existente, cumpliendo la regla de *Seguridad -> Roles de usuario*.
- `GET /api/admin/trabajadores/{idUsuario}`:
  - Devuelve el legajo completo de un trabajador específico, incluyendo todos sus documentos cargados, los documentos obligatorios faltantes y el historial reciente de verificaciones/accesos (`verificaciones`).
  - **Auditoría de Acceso RRHH (Ley N° 25.326):** Cada vez que un usuario de RRHH abre el legajo de un trabajador (`GET /api/admin/trabajadores/{idUsuario}`, con `id_documento = null`) o abre/descarga uno de sus certificados (`GET /api/documentos/{id}/archivo`, guardando `id_documento = id`), se registra automáticamente en la tabla `verificaciones` una fila con `metodo = "PANEL_RRHH"`, `verificador_usuario_id = idAdminActual`, `id_documento`, `ip_verificador` y `fecha_verificacion = UtcNow`.
- `PATCH /api/admin/trabajadores/{idUsuario}/estado-laboral`:
  - Permite activar o dar de baja (`activo = true / false`) a un trabajador. Si pasa a `false`, invalida sus sesiones activas en la tabla `sesiones` y su QR pasa inmediatamente a devolver `ROJO (TRABAJADOR INACTIVO)` en garita.
- `POST /api/admin/trabajadores/{idUsuario}/resetear-contrasena` (Contingencia `C-09`):
  - Permite a RRHH blanquear/generar una contraseña temporal de forma presencial cuando el trabajador olvidó su clave y ya no tiene acceso a su correo electrónico registrado, invalidando además sus sesiones previas.
- `GET /api/admin/trabajadores/exportar-csv`:
  - Genera y descarga un archivo `.csv` (UTF-8 con BOM para apertura directa en Microsoft Excel) con el padrón filtrado (DNI, Nombre Completo, Especialidad, Teléfono, Email, Estado Laboral, Semáforo General, Documentos Vigentes, Pendientes, Vencidos/Faltantes).

### 2.2. Auditoría Documental y Notificación Manual (`HU-16`)
- `PATCH /api/admin/documentos/{idDocumento}/revisar`:
  - Body: `{ "estadoRevision": "VALIDADO" | "OBSERVADO", "observacionRrhh": "..." }`.
  - Si `estadoRevision == "OBSERVADO"`, exige que `observacionRrhh` no esté vacío.
  - Actualiza el documento y genera automáticamente una alerta (`origen = "MANUAL_RRHH"`) para el trabajador informando si su documento fue validado u observado (incluyendo el motivo).
  - Si el trabajador tiene `recibir_alertas_email == true`, encola/envía también el aviso por correo electrónico.
- `POST /api/admin/trabajadores/{idUsuario}/notificar`:
  - Body: `{ "mensaje": "...", "idDocumento": null | int }`.
  - Permite a RRHH enviar en cualquier momento una notificación manual directa al trabajador (por ejemplo, *"Tenés el examen médico vencido, acercate a RRHH antes del jueves"*), registrándola en `alertas` con `origen = "MANUAL_RRHH"` para visualización en Dashboard, Push y Email.

### 2.3. Carga Delegada de Documentos por RRHH y Catálogo (`HU-17`)
- `POST /api/admin/trabajadores/{idUsuario}/documentos` (`multipart/form-data`):
  - Permite a RRHH subir o actualizar un certificado directamente en el legajo de un empleado.
  - Registra `subido_por_id = idAdminActual` y `estado_revision = "VALIDADO"` de forma automática.
- `POST /api/admin/tipos-documento`, `PUT /api/admin/tipos-documento/{id}` y `PATCH /api/admin/tipos-documento/{id}/activo`:
  - ABM completo del catálogo `tipos_documento` (nombre, descripción, `obligatorio`, `dias_aviso`, `activo`) para cumplir con la Contingencia `C-14` sin necesidad de reprogramar.

---

## 3. Endpoints de Alertas del Trabajador (`AlertaController`)
- `GET /api/alertas/mis-alertas`:
  - Devuelve las alertas del trabajador autenticado ordenadas de más reciente a más antigua, indicando cuántas están sin leer (`fecha_leida == null`).
- `PATCH /api/alertas/{idAlerta}/leer`:
  - Marca `fecha_leida = DateTime.UtcNow`.
- `PATCH /api/alertas/leer-todas`:
  - Marca todas las alertas pendientes del trabajador como leídas.

---

## 4. Procesos en Segundo Plano (`BackgroundService`)

### 4.1. `AlertaVencimientoBackgroundService` (`HU-08`)
- Se ejecuta al iniciar el servidor y luego cada 24 horas (o intervalo configurable).
- Recorre todos los documentos de usuarios con `activo == true` y `tiene_vencimiento == true`:
  1. Actualiza la columna `documento.estado` (`VIGENTE`, `PROXIMO`, `VENCIDO`) según la diferencia entre `fecha_vencimiento` y la fecha actual.
  2. Evalúa los 4 umbrales oficiales de la tesis: **`15`, `7`, `3` y `0` días** antes del vencimiento.
  3. **Regla de Idempotencia:** Cuando los días restantes coinciden con uno de los umbrales (o si `diasRestantes <= 15` y aún no se emitió la alerta del umbral correspondiente), verifica en la tabla `alertas` si ya existe un registro con `(id_documento, dias_anticipacion == umbral, origen == "AUTOMATICA")` posterior a `documento.fecha_actualizacion`.
  4. Si no existe, crea la alerta en base de datos (`tipo_alerta = "PUSH"` / `"DASHBOARD"`, implementada como notificación in-app en la PWA sin requerir FCM/Web Push externo, tal como define `HU-08`) y, si `usuario.recibir_alertas_email == true`, dispara el envío por email (`EmailService` con soporte SMTP/SendGrid y registro de alerta con `tipo_alerta = "EMAIL"`).

### 4.2. `BackupService`, Retención Documental (`C-11`) y Restauración (`C-05`, `RNF 08`)
- **Backup Real + Verificación de Restore (`C-05`):**
  - Ejecuta **`pg_dump` real** mediante `System.Diagnostics.Process` generando un archivo `.sql` / `.backup` con timestamp en la carpeta de respaldos (`/var/www/backups/` en DonWeb VPS o `./storage/backups/` en local), mide el peso real del archivo generado en MB (`tamano_mb`) y verifica su integridad (mediante `pg_restore --list` o validación de encabezado SQL) antes de marcar `exitoso = true` en la tabla `backups`.
  - Si en un entorno de desarrollo local el binario `pg_dump` no está en el `PATH`, incluye un fallback que exporta un snapshot JSON/SQL de las tablas vía EF Core y valida su lectura.
- **Copia Fuera del VPS (`C-05`, `RNF 08`):**
  - Expone `GET /api/admin/backups`, `POST /api/admin/backups/ejecutar`, `GET /api/admin/backups/{id}/descargar` (para que el administrador descargue la copia fuera del VPS a su máquina local o almacenamiento externo) y `POST /api/admin/backups/{id}/verificar-restore` (que ejecuta una prueba de validación/restauración no destructiva del archivo de backup), además de soportar envío automático externo configurable (`ExternalBackupWebhookUrl` / S3 / SFTP).
- **Política de Retención y Archivado > 2 Años (Contingencia `C-11`):**
  - El job diario ejecuta `AplicarPoliticaRetencionAsync()`: **jamás elimina ni archiva documentos de trabajadores activos (`activo == true`)**.
  - Aplica únicamente a **usuarios inactivos (`activo == false`) cuya última actividad (`ultimo_acceso` o `fecha_actualizacion`) supere los 2 años (`730 días`)** y cuyos documentos estén vencidos hace **más de 2 años (`730 días`)**.
  - Por resguardo legal, **no borra los archivos sin respaldo**: los empaqueta y mueve a un paquete comprimido `.zip` dentro de `/var/www/backups/archivo_historico/` (incluido en los backups descargables fuera del VPS) y actualiza `ruta_archivo = "ARCHIVADO_ZIP:{nombre_zip}"`, liberando espacio en el disco activo sin perder el documento histórico.

---

## 5. Checklist de Verificación (Definition of Done — Parte 3)
- [ ] Un usuario con rol `TRABAJADOR` recibe `HTTP 403 Forbidden` si intenta llamar a cualquier endpoint `/api/admin/*`.
- [ ] RRHH puede marcar un documento como `OBSERVADO` con motivo; el semáforo del trabajador pasa a `ROJO` y el trabajador recibe la alerta con el motivo.
- [ ] RRHH puede cargar un documento en el legajo de un trabajador y este nace como `VALIDADO`.
- [ ] RRHH puede enviar una notificación manual a cualquier trabajador y exportar el padrón en `.csv`.
- [ ] `AlertaVencimientoBackgroundService` genera alertas a 15, 7, 3 y 0 días sin duplicarlas si el servidor se reinicia.
