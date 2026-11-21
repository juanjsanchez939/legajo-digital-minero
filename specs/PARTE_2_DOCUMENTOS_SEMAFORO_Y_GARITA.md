# PARTE 2: Gestión Documental, Motor del Semáforo y Verificación en Garita

> ⚠️ **REVISAR DOCUMENTACIÓN PARA IMPLEMENTACIÓN:**  
> - Revisar **"HISTORIAS DE USUARIO (HU-05, HU-06, HU-07, HU-09, HU-10, HU-12, HU-13, HU-18)"** en [TFT_Entrega_2_Actualizado.docx](file:///C:/Users/juan-dev/Desktop/PF/TFT_Entrega_2_Actualizado.docx).  
> - Revisar **"Tabla de Contingencias (C-08, C-11, C-12)"** y sección **"Cifrado y Protección de Datos (Enmascaramiento en QR)"** en [TFT_Entrega_2_Actualizado.docx](file:///C:/Users/juan-dev/Desktop/PF/TFT_Entrega_2_Actualizado.docx).

---

## 1. Objetivo de esta Parte
Implementar en el backend (.NET 10) la subida, validación, visualización y reemplazo físico de documentos (`JPG`, `PNG`, `PDF` hasta 5 MB), el motor de cálculo automático del **Semáforo de Aptitud** y los endpoints de generación y verificación pública en Garita (tanto por **Código QR** como por **DNI**).

---

## 2. Reglas de Gestión de Archivos (`AlmacenamientoService` y `DocumentoService`)

1. **Validación estricta y límite en Kestrel (`HU-13`):**
   - Configurar en Kestrel y en los endpoints `multipart/form-data` `[RequestSizeLimit(5_500_000)]` y `FormOptions.MultipartBodyLengthLimit = 5_500_000` para que el servidor rechace a nivel de transporte cualquier request pesado antes de procesarlo en memoria.
   - Extensiones y MIME types permitidos únicamente: `.jpg` / `.jpeg` (`image/jpeg`), `.png` (`image/png`), `.pdf` (`application/pdf`).
   - Validar también la firma mágica (magic bytes) del archivo para impedir ejecutables renombrados como `.pdf` (riesgo documentado en `HU-13`).
   - Tamaño máximo por archivo: **5 MB** (`5 * 1024 * 1024` bytes).
2. **Almacenamiento seguro fuera del root público:**
   - Ruta configurable en `appsettings.json`: `"Storage:DocumentsPath"` (en desarrollo `./storage/documentos`, en DonWeb VPS `/var/www/documentos/`).
   - Los archivos **nunca** se sirven como archivos estáticos públicos; solo pueden descargarse/visualizarse a través de `GET /api/documentos/{id}/archivo` validando el JWT (solo el propietario del documento o un usuario con rol `ADMINISTRADOR`).
3. **Unicidad y reemplazo físico (`HU-12` y `C-11`):**
   - Cada trabajador tiene como máximo 1 registro por `id_tipo_documento`.
   - Si el trabajador o RRHH sube un documento para un `id_tipo_documento` que ya existía en su legajo:
     - Se elimina físicamente del disco el archivo anterior (`File.Delete`).
     - Se guarda el nuevo archivo con nombre único (`{id_usuario}_{id_tipo_documento}_{Guid}.{ext}`).
     - Si lo subió el trabajador (`rol == "TRABAJADOR"`), `estado_revision` pasa a `"PENDIENTE"` y `observacion_rrhh = null`.
     - Si lo subió RRHH (`rol == "ADMINISTRADOR"`), `estado_revision` nace directamente como `"VALIDADO"`.
     - Se eliminan o archivan las alertas de vencimiento previas de ese documento para iniciar el nuevo ciclo limpio.
4. **Eliminación (`HU-12`):**
   - Un trabajador solo puede eliminar un documento si su `TIPO_DOCUMENTO.obligatorio == false`. Los documentos obligatorios no se pueden eliminar, solo actualizar/reemplazar.

---

## 3. Motor de Cálculo del Semáforo (`UsuarioService` / `DocumentoService`)

### 3.1. Estado de Vigencia de cada Documento (`documento.estado`)
- **Cálculo de `"hoy"`:** Obtener siempre la fecha actual en la zona horaria **`America/Argentina/San_Juan` (UTC-3)**:
  `DateOnly hoy = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, sanJuanTz));`
- Si `tiene_vencimiento == false`: siempre `"VIGENTE"`.
- Si `fecha_vencimiento < hoy`: `"VENCIDO"`.
- Si `fecha_vencimiento >= hoy` y `(fecha_vencimiento.Value.DayNumber - hoy.DayNumber) <= tipoDocumento.dias_aviso` (default 15 días): `"PROXIMO"`.
- Si `(fecha_vencimiento.Value.DayNumber - hoy.DayNumber) > tipoDocumento.dias_aviso`: `"VIGENTE"`.

### 3.2. Semáforo General del Trabajador (`VERDE` / `AMARILLO` / `ROJO`)
El método `CalcularEstadoDocumental(int idUsuario)` evalúa en este orden exacto:
1. **Validación de Baja Laboral:** Si `usuario.activo == false` → retorna **🔴 `ROJO`** (`motivo: "TRABAJADOR INACTIVO / NO HABILITADO"`).
2. **Control de Obligatorios Faltantes:** Obtiene todos los `tipos_documento` con `activo == true` y `obligatorio == true`. Si al trabajador le falta cargar alguno → retorna **🔴 `ROJO`**.
3. **Control de Vencidos u Observados:** Si algún documento cargado tiene `estado == "VENCIDO"` o `estado_revision == "OBSERVADO"` → retorna **🔴 `ROJO`**.
4. **Control de Próximos a Vencer o Pendientes de Revisión RRHH:** Si algún documento cargado tiene `estado == "PROXIMO"` o `estado_revision == "PENDIENTE"` → retorna **🟡 `AMARILLO`**.
5. **Apto Total:** Si todos los documentos obligatorios están cargados, `"VIGENTE"` y con `estado_revision == "VALIDADO"` → retorna **🟢 `VERDE`**.

---

## 4. Endpoints a Implementar en la Parte 2

### 4.1. Documentos y Dashboard del Trabajador
- `GET /api/tipos-documento`: Lista el catálogo de tipos de documentos activos.
- `POST /api/documentos/cargar` (`multipart/form-data`, `HU-05`, `HU-06`, `HU-13`):
  - Recibe `idTipoDocumento`, `tieneVencimiento`, `fechaVencimiento` (obligatoria y `>= hoy` si `tieneVencimiento == true`), `descripcion` opcional y `archivo`.
- `PUT /api/documentos/{id}` (`multipart/form-data`, `HU-12`): Reemplaza archivo y/o actualiza fecha de vencimiento.
- `DELETE /api/documentos/{id}` (`HU-12`): Solo permite borrar si el tipo de documento no es obligatorio.
- `GET /api/documentos/{id}/archivo`: Devuelve el stream del PDF/imagen validando JWT (dueño o RRHH).
- `GET /api/usuarios/mi-dashboard` (`HU-07`, `HU-14`):
  - Devuelve semáforo general (`VERDE`, `AMARILLO`, `ROJO`), contadores (`vigentes`, `proximos`, `vencidos`, `pendientes`, `observados`, `faltantes`), lista completa de documentos con su estado y lista de obligatorios faltantes, más `fechaSincronizacionUtc`.

### 4.2. Código QR y Verificación Pública en Garita (`HU-09`, `HU-10`, `HU-18`)
- `GET /api/qr/mi-codigo` (`HU-09`):
  - Devuelve el `qr_token` del trabajador autenticado y la URL pública de verificación (`{FrontendBaseUrl}/verificacion/{qr_token}`).
- `GET /api/verificacion/qr/{qrToken:guid}` (Público, sin login, con **Rate Limiting de 30 req/min por IP** — `HU-10`):
  - Busca al usuario por `qr_token`.
  - Registra el evento en la tabla `verificaciones` con `metodo = "QR"`, `ip_verificador`, `estado_verificado` y `fecha_verificacion = UtcNow`.
  - Devuelve **únicamente datos enmascarados** (Ley N° 25.326 y `HU-10`): `nombreCompleto`, `dniEnmascarado` (ej. `**.***.456`, mostrando solo los últimos 3 dígitos), `especialidad`, `fotoPerfilUrl`, `activo`, `semaforoGeneral` (`VERDE`/`AMARILLO`/`ROJO`), `fechaHoraConsultaUtc` y la lista de documentos con `{ tipoNombre, estadoVigencia, estadoRevision, fechaVencimiento }` (**jamás** devuelve `ruta_archivo` ni links a los PDFs).
- `GET /api/verificacion/dni/{dni}` (Público, sin login, con **Rate Limiting de 30 req/min por IP** — `HU-18` / Contingencia `C-08`):
  - Busca al usuario por `dni` exacto, registra en `verificaciones` con `metodo = "DNI"` y devuelve exactamente el mismo DTO enmascarado (`dniEnmascarado`) que la verificación por QR.

---

## 5. Checklist de Verificación (Definition of Done — Parte 2)
- [ ] Subida rechaza archivos `> 5 MB` o con formato distinto a `JPG/PNG/PDF`.
- [ ] Al renovar un documento existente, el archivo viejo se borra del disco y no se duplica la fila en `documentos`.
- [ ] El cálculo del semáforo devuelve `ROJO` si falta un documento obligatorio, si está vencido, si fue `OBSERVADO` por RRHH o si el usuario tiene `activo = false`.
- [ ] Los endpoints públicos `/api/verificacion/qr/{qrToken}` y `/api/verificacion/dni/{dni}` registran la auditoría en `verificaciones` y no exponen archivos originales.
