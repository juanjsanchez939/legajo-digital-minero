# Plan Maestro de Desarrollo — "Legajo en el bolsillo"

> ⚠️ **REVISAR DOCUMENTACIÓN PARA IMPLEMENTACIÓN:**  
> Antes de ejecutar cada parte, el agente de programación debe consultar:
> - **Documento maestro de tesis:** [TFT_Entrega_2_Actualizado.docx](file:///C:/Users/juan-dev/Desktop/PF/TFT_Entrega_2_Actualizado.docx) (contiene el detalle narrativo de las 18 Historias de Usuario, Tabla de Contingencias `C-01` a `C-14`, matrices de Seguridad y los 4 diagramas oficiales de DER, Arquitectura, Despliegue y Seguridad).
> - **Referencia visual UI del trabajador:** [prototipo.png](file:///C:/Users/juan-dev/Desktop/PF/prototipo.png) (diseño base del Dashboard móvil del operario minero; ignorar textos residuales de mockup como `[cite: ...]` o etiquetas en inglés `"Green"`/`"Yellow"` y usar textos reales en español: `"Vigente"`, `"Próximo a vencer"`, `"Vencido"`).

---

## 1. Resumen Arquitectónico y Stack Oficial

| Capa | Tecnología | Notas de Implementación |
|---|---|---|
| **Frontend** | `React 19 + Vite + TypeScript` (PWA) | `ServiceWorker` (`vite-plugin-pwa`) + `IndexedDB` para modo offline en alta montaña. |
| **Backend** | `C# / ASP.NET Core 10 Web API` | Arquitectura en capas: `Controllers` → `Services` → `Repositories` (`EF Core 10`). |
| **Base de Datos** | `PostgreSQL 16` | 7 entidades relacionales con integridad referencial y restricciones únicas. |
| **Almacenamiento** | Disco local VPS (`/var/www/documentos/`) | Configurable por `appsettings.json` (carpeta local en desarrollo, `/var/www/documentos/` en producción). |
| **Infraestructura** | `DonWeb Cloud Server (VPS)` | 2 vCPU, 4 GB RAM, 50 GB SSD, Linux + `Nginx` (Reverse Proxy + SSL Let's Encrypt) + `Docker Compose` opcional. |

---

## 2. Reglas de Negocio Inquebrantables (Checklist Global)

1. **Prevención de Mass Assignment en Roles (`HU-01`):** El endpoint público de registro fuerza siempre `rol = "TRABAJADOR"` en el servidor. El rol `"ADMINISTRADOR"` (RRHH) solo se crea por *Database Seeder* inicial o por otro Administrador autenticado.
2. **Baja Laboral Inmediata (`HU-10`, `HU-15`, `HU-18`):** Si RRHH desactiva a un empleado (`activo = false`), cualquier escaneo de su QR o búsqueda por DNI en garita debe devolver inmediatamente **🔴 ROJO — TRABAJADOR INACTIVO / NO HABILITADO**, aunque sus certificados tengan fecha futura.
3. **Lógica del Semáforo de Aptitud (`HU-07`, `HU-16`):**
   - 🔴 **ROJO (`NO APTO`):** `activo == false`, o falta al menos 1 documento obligatorio (`TIPO_DOCUMENTO.obligatorio == true`), o al menos 1 documento obligatorio está `VENCIDO` (`fecha_vencimiento < hoy`), o fue `OBSERVADO` (rechazado) por RRHH.
   - 🟡 **AMARILLO (`ATENCIÓN`):** No cumple ninguna condición de Rojo, pero al menos 1 documento vence en `<= 15 días` (`PROXIMO`) o tiene `estado_revision == "PENDIENTE"`.
   - 🟢 **VERDE (`APTO`):** Usuario `activo == true` y todos los documentos obligatorios están cargados, `VIGENTES` (`> 15 días` o `tiene_vencimiento == false`) y con `estado_revision == "VALIDADO"`.
4. **Cuidado de los 50 GB del VPS (`HU-12`, `C-11`):** Existe un único registro activo por `(id_usuario, id_tipo_documento)`. Al renovar o reemplazar un documento, el backend elimina físicamente el archivo anterior del disco antes de guardar el nuevo.
5. **Modo Offline Realista + Anti-Captura (`HU-11`):** La PWA guarda un snapshot en `IndexedDB`, recalcula localmente el semáforo comparando `fecha_vencimiento` contra el reloj del teléfono (incluso sin internet) y muestra un **reloj en vivo con segundos** en la pantalla del QR y Dashboard para impedir el uso de capturas de pantalla viejas.
6. **Idempotencia de Alertas (`HU-08`):** El `BackgroundService` diario verifica que no exista ya una alerta para `(id_documento, dias_anticipacion)` antes de emitirla (`15`, `7`, `3` y `0` días).

---

## 2.1. Decisiones Técnicas Unificadas (Word ↔ `.md`)

| Tema | Decisión Cerrada para Implementación |
|---|---|
| **Cifrado AES-256 en BD** | **No cifrar `dni` ni `email`** (se indexan con `UNIQUE` para login y búsqueda en garita `HU-18`). **Cifrar con AES-256** (mediante el mismo `AesEncryptionConverter` de EF Core) los campos `usuarios.telefono`, `documentos.descripcion`, `documentos.observacion_rrhh` y `alertas.mensaje`. |
| **DNI en Perfil (`RF 04` / `HU-04`)** | El `dni` es **estrictamente de solo lectura** luego del registro (`HU-04`). |
| **Notificaciones Push (`HU-08`)** | Se implementan como **notificaciones in-app** (alertas en tiempo real / bandeja y banner dentro de la PWA, tal como define `HU-08`) + **correo electrónico (SMTP/SendGrid)**, sin dependencia de Web Push/FCM externo. |
| **Backups (`BackupService`)** | Ejecuta **`pg_dump` real** hacia la carpeta de backups (`/var/www/backups/` o `./storage/backups/`), calcula el tamaño real en MB y registra el resultado en la tabla `backups` (con fallback controlado si `pg_dump` no está instalado en el entorno local de desarrollo). |
| **Vista Pública QR / DNI (`HU-10`)** | Muestra nombre completo, foto, especialidad, **DNI parcialmente enmascarado** (ej. `**.***.456`), semáforo general, fecha/hora de consulta y la lista de documentos con su estado (`VIGENTE`/`PROXIMO`/`VENCIDO` y estado de revisión), **nunca** los archivos PDF/imágenes originales. |
| **Zona Horaria Oficial** | Todos los `DateTime` de auditoría se guardan en **UTC** (`timestamptz`), y la fecha `"hoy"` para cálculo de vencimientos y semáforo se evalúa siempre en la zona horaria **`America/Argentina/San_Juan` (UTC-3)**. |
| **Duración de Tokens** | **Access Token JWT:** `30 minutos` \| **Refresh Token:** `7 días` (rotativo) \| **Token Recuperación Clave:** `60 minutos` (1 hora). |
| **Valores de Rate Limiting** | `/api/auth/*`: `10 req/min por IP` \| `/api/verificacion/*` (QR y DNI): `30 req/min por IP` \| General API: `120 req/min por IP`. |
| **Creación de otros Admins** | Endpoint `POST /api/admin/usuarios/crear-admin` y promoción de rol (solo accesible por un `ADMINISTRADOR` autenticado). |
| **Consentimiento Ley 25.326** | Columna `usuarios.consentimiento_datos_fecha` (`DATETIME NOT NULL`) y checkbox obligatorio al registrarse (`HU-01`). |
| **Offline con JWT vencido** | Si `navigator.onLine == false` y existe snapshot en `IndexedDB`, el `AuthGuard` **no redirige al login** aunque el JWT de 30 min haya expirado: habilita modo offline de solo lectura (`HU-11`). |
| **IP Real tras Nginx** | `UseForwardedHeaders` (`X-Forwarded-For`, `X-Real-IP`) en `.NET 10` + cabeceras en `nginx.conf` para que Rate Limiting y Auditoría registren la IP real del cliente. |
| **Límite 5 MB en Red** | `client_max_body_size 6M` en `Nginx` + `[RequestSizeLimit(5_500_000)]` y `MultipartBodyLengthLimit` en `Kestrel` para frenar archivos gigantes antes de llegar al servicio. |
| **Backup Externo + Restore** | Descarga fuera del VPS (`GET /api/admin/backups/{id}/descargar` + envío externo configurable) y verificación automática del dump con `pg_restore --list` (`C-05`, `RNF 08`). |
| **CI/CD y Tests (`RNF 10`)** | Proyecto `LegajoMinero.Tests` (`xUnit`) + GitHub Actions (`.github/workflows/ci.yml`) que corre `dotnet test` y `npm run build`. |
| **Rendimiento `< 3 s` (`RNF 03`)** | Índices compuestos en PostgreSQL, paginación en `GET /api/admin/trabajadores` y prueba de rendimiento con 500 usuarios simulados. |
| **Auditoría de Accesos RRHH** | `verificaciones.verificador_usuario_id` (`INT NULL`, FK), `verificaciones.id_documento` (`INT NULL`, FK) y `metodo = "PANEL_RRHH"` registran quién de RRHH, cuándo, desde qué IP y qué documento puntual abrió. |
| **Retención > 2 Años (`C-11`)** | Job diario **archiva en `.zip` comprimido** (no borra sin respaldo) los archivos de documentos vencidos `> 2 años` únicamente de **usuarios inactivos (`activo == false`) > 2 años**, cuidando los 50 GB SSD sin riesgo legal. |

---

## 3. Matriz de Trazabilidad Completa (RF ↔ HU ↔ Contingencias ↔ Parte)

> ⚠️ **REVISAR DOCUMENTACIÓN PARA IMPLEMENTACIÓN:** Esta matriz asegura que las 18 Historias de Usuario, los 18 Requerimientos Funcionales, los 12 No Funcionales y las 14 Contingencias de [TFT_Entrega_2_Actualizado.docx](file:///C:/Users/juan-dev/Desktop/PF/TFT_Entrega_2_Actualizado.docx) estén cubiertos en el código.

| RF / HU | Funcionalidad | Contingencias cubiertas | Servicio Backend (de los 10 oficiales) | Parte |
|---|---|---|---|---|
| `RF 01` / `HU-01` | Registro de usuarios y foto de perfil | `C-06` | `AuthService`, `UsuarioService` | Parte 1 y 4 |
| `RF 02` / `HU-02` | Inicio de sesión + bloqueo 5 intentos (15 min) | `C-06`, `C-13` | `AuthService`, `SesionService` | Parte 1 y 4 |
| `RF 03` / `HU-03` | Recuperación de contraseña (link 1h + reset por RRHH) | `C-03`, `C-09` | `AuthService` | Parte 1, 3 y 4 |
| `RF 04` / `HU-04` | Gestión de perfil y cierre remoto de sesiones | `C-13` | `UsuarioService`, `SesionService` | Parte 1 y 4 |
| `RF 05` / `HU-05` | Carga de documentos (JPG, PNG, PDF <= 5 MB) | `C-11`, `C-12` | `DocumentoService`, `AlmacenamientoService` | Parte 2 y 4 |
| `RF 06` / `HU-06` | Registro de vencimientos (con opción "Sin vencimiento") | `C-12` | `DocumentoService` | Parte 2 y 4 |
| `RF 07` / `HU-07` | Visualización del estado documental (Semáforo) | `C-02` | `UsuarioService`, `DocumentoService` | Parte 2 y 4 |
| `RF 08` / `HU-08` | Sistema de alertas proactivas (15, 7, 3 y 0 días) | `C-03`, `C-04` | `AlertaService` (`BackgroundService`) | Parte 3 y 4 |
| `RF 09` / `HU-09` | Generación de código QR dinámico (`qr_token` UUID) | `C-08` | `QRService` | Parte 2 y 4 |
| `RF 10` / `HU-10` | Verificación pública mediante escaneo QR en garita | `C-07`, `C-08` | `VerificacionService` | Parte 2 y 4 |
| `RF 11` / `HU-11` | Visualización offline (`ServiceWorker` + `IndexedDB`) | `C-02` | PWA Frontend + `IndexedDB` | Parte 4 |
| `RF 12` / `HU-12` | Administración de documentos (reemplazo físico en disco) | `C-11`, `C-12` | `DocumentoService`, `AlmacenamientoService` | Parte 2 y 4 |
| `RF 13` / `HU-13` | Validación de formatos, tamaño y firma mágica (MIME) | `C-06`, `C-12` | `AlmacenamientoService` | Parte 2 y 4 |
| `RF 14` / `HU-14` | Panel principal (Dashboard del trabajador) | `C-02`, `C-04` | `UsuarioService` | Parte 2 y 4 |
| `RF 15` / `HU-15` | Panel de monitoreo de padrón y exportación CSV/PDF (RRHH) | `C-01`, `C-13` | `UsuarioService` (`AdminController`) | Parte 3 y 5 |
| `RF 16` / `HU-16` | Auditoría (`VALIDADO`/`OBSERVADO`) y alertas manuales (RRHH) | `C-03`, `C-04`, `C-12` | `DocumentoService`, `AlertaService` | Parte 3 y 5 |
| `RF 17` / `HU-17` | Carga delegada por RRHH y ABM de `TIPO_DOCUMENTO` | `C-14` | `TipoDocumentoService`, `DocumentoService` | Parte 3 y 5 |
| `RF 18` / `HU-18` | Verificación alternativa por DNI en garita | `C-08` | `VerificacionService` | Parte 2 y 4 |
| `RNF 08` / `C-05` | Respaldo automático y registro de copias de seguridad | `C-01`, `C-05` | `BackupService` | Parte 3 y 5 |

---

## 4. Los 10 Servicios del Backend exigidos por la Arquitectura de la Tesis

Para que el código fuente coincida 1 a 1 con el **Diagrama de Arquitectura (`image3.png`)** y la tabla **Componentes del Backend (Servicios)** de [TFT_Entrega_2_Actualizado.docx](file:///C:/Users/juan-dev/Desktop/PF/TFT_Entrega_2_Actualizado.docx), la capa `Application/Services` debe implementar exactamente estos 10 servicios:
1. `AuthService` (Parte 1)
2. `UsuarioService` (Parte 1, 2 y 3)
3. `SesionService` (Parte 1 — incluye cierre remoto de sesiones para Contingencia `C-13`)
4. `DocumentoService` (Parte 2 y 3)
5. `TipoDocumentoService` (Parte 2 y 3 — cubre Contingencia `C-14`)
6. `AlmacenamientoService` (Parte 2)
7. `QRService` (Parte 2)
8. `VerificacionService` (Parte 2)
9. `AlertaService` (Parte 3)
10. `BackupService` (Parte 3)

---

## 5. Hoja de Ruta en 5 Partes

1. **[PARTE 1 — Base de Datos, Arquitectura Backend (.NET 10) y Autenticación](./specs/PARTE_1_BASE_DATOS_Y_AUTH.md)** (`HU-01` a `HU-04`)
2. **[PARTE 2 — Gestión Documental, Motor del Semáforo y Verificación en Garita](./specs/PARTE_2_DOCUMENTOS_SEMAFORO_Y_GARITA.md)** (`HU-05` a `HU-07`, `HU-09`, `HU-10`, `HU-12`, `HU-13`, `HU-18`)
3. **[PARTE 3 — Módulo Backend de RRHH y Motor de Alertas](./specs/PARTE_3_MODULO_RRHH_Y_ALERTAS.md)** (`HU-08`, `HU-15`, `HU-16`, `HU-17`)
4. **[PARTE 4 — Frontend PWA del Trabajador, Modo Offline y Vista de Garita](./specs/PARTE_4_FRONTEND_PWA_TRABAJADOR_Y_OFFLINE.md)** (`HU-01` a `HU-14`, `HU-18`)
5. **[PARTE 5 — Panel Web de RRHH, Despliegue en DonWeb VPS y Cierre de Tesis](./specs/PARTE_5_PANEL_RRHH_DEPLOY_DONWEB_Y_CIERRE.md)** (`HU-15` a `HU-17` + Deploy + Sección C del Word)
