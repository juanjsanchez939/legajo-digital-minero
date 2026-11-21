# PARTE 5: Panel Web de RRHH, Despliegue en DonWeb VPS y Cierre de Tesis

> ⚠️ **REVISAR DOCUMENTACIÓN PARA IMPLEMENTACIÓN:**  
> - Revisar **"HISTORIAS DE USUARIO (HU-15, HU-16, HU-17)"**, **"Diagrama de Despliegue (`image4.png`)"** y **"COSTO DE IMPLEMENTACIÓN (DonWeb Cloud Server)"** en [TFT_Entrega_2_Actualizado.docx](file:///C:/Users/juan-dev/Desktop/PF/TFT_Entrega_2_Actualizado.docx).  
> - Al finalizar el código de esta parte, revisar las secciones pendientes de la **Sección C** (`AGRADECIMIENTOS`, `DESARROLLO E IMPLEMENTACIÓN`, `PARTE 4 | MANUAL DE USUARIO`, `PARTE 5 | CONCLUSIONES` y `BIBLIOGRAFÍA`) en [TFT_Entrega_2_Actualizado.docx](file:///C:/Users/juan-dev/Desktop/PF/TFT_Entrega_2_Actualizado.docx).

---

## 1. Objetivo de esta Parte
Construir el **Panel Web de Administración / RRHH** en el frontend (`HU-15`, `HU-16`, `HU-17`), dejar lista la configuración de despliegue para **DonWeb Cloud Server (VPS)** (`Docker Compose` + `Nginx` + SSL) y completar las secciones finales del documento de tesis con capturas reales del sistema funcionando.

---

## 2. Vistas del Panel de RRHH (`frontend/src/pages/admin/`)

Cuando un usuario inicia sesión con `rol == "ADMINISTRADOR"`, es redirigido al **Panel de Control de RRHH** (diseño adaptable a escritorio y tablet/móvil):

### 2.1. Dashboard de Padrón y Exportación de Nómina (`HU-15`)
- **Tarjetas de resumen superior:**
  - Total de trabajadores registrados.
  - 🟢 Total **Aptos (Verde)**.
  - 🟡 Total **En Atención / Pendientes de revisión (Amarillo)**.
  - 🔴 Total **Inhabilitados / Vencidos / Observados (Rojo)**.
- **Barra de herramientas:**
  - Buscador en tiempo real por **DNI**, **Nombre** o **Especialidad**.
  - Filtros rápidos por color de semáforo (`Todos`, `Verde`, `Amarillo`, `Rojo`, `Con documentos pendientes de revisión`, `Inactivos`).
  - Botón **"Exportar Nómina CSV (Excel)"** y botón **"Imprimir / Exportar Nómina PDF"** (exporta la tabla filtrada actual, ideal para armar el manifiesto de cuadrilla que sube a mina).
- **Tabla de Trabajadores:**
  - Columnas: Foto + Nombre, DNI, Especialidad, Semáforo General, Resumen Documental (`Vigentes / Pendientes / Vencidos`), Estado Laboral (`Activo` / `Baja`) y botón **"Ver Legajo"**.

### 2.2. Vista de Detalle y Auditoría del Legajo de un Trabajador (`HU-16`, `HU-17`)
Al hacer clic en **"Ver Legajo"** de un operario:
- **Encabezado del trabajador:** Datos personales, semáforo actual, switch/botón **"Dar de Baja / Reactivar Trabajador"** (`activo = true/false`) y botón **"Enviar Notificación Manual"** (abre modal para escribir un mensaje directo al trabajador que impacta en sus alertas del Dashboard, Push y Email).
- **Grilla de Documentos del Trabajador:**
  - Muestra todos los documentos cargados y los obligatorios faltantes.
  - Para cada documento cargado:
    - Botón **"Ver Archivo Original"** (abre el PDF o imagen en un visor modal o pestaña segura).
    - Botón **"✅ Validar Documento"** (cambia `estado_revision` a `VALIDADO` y recalcula el semáforo al instante).
    - Botón **"⚠️ Observar / Rechazar"** (abre modal pidiendo el motivo obligatorio, cambia `estado_revision` a `OBSERVADO`, pone el semáforo en `ROJO` y notifica automáticamente al trabajador).
  - Para cualquier tipo de documento (cargado o faltante):
    - Botón **"📤 Subir / Actualizar por RRHH"** (`HU-17`): permite a RRHH cargarle el certificado directamente al empleado (quedando automáticamente `VALIDADO`).
- **Historial de Verificaciones en Garita:**
  - Tabla inferior con los últimos escaneos QR o consultas por DNI de ese trabajador (fecha, hora, método `QR`/`DNI`, IP y semáforo resultante).

### 2.3. Gestión del Catálogo de Tipos de Documentos y Backups (`HU-17`)
- Pestaña **"Tipos de Documentos"**:
  - Tabla con los tipos de documentos (`Examen Médico`, `Carnet Alta Montaña`, etc.).
  - Botón para crear un nuevo tipo de documento o editar uno existente (`nombre`, `descripcion`, switch `obligatorio`, `dias_aviso`, switch `activo`).
- Pestaña **"Backups y Auditoría"**:
  - Lista de registros de la tabla `backups` y botón para disparar un registro de respaldo manual.

---

## 3. Configuración de Despliegue en DonWeb Cloud Server (VPS), CI/CD y Prueba de Rendimiento

Crear en la raíz del proyecto los archivos listos para producción en el VPS Linux de DonWeb (2 vCPU, 4 GB RAM, 50 GB SSD):
1. `backend/Dockerfile`: Imagen multi-stage liviana de `.NET 10` (`mcr.microsoft.com/dotnet/aspnet:10.0` y `sdk:10.0`).
2. `frontend/Dockerfile` + `deploy/nginx.conf`:
   - Compila la PWA con Vite, sirve los estáticos con headers de caché correctos para `sw.js` y `manifest.webmanifest`, y configura `proxy_pass` de `/api/` hacia el backend `.NET 10`.
   - **Límite de subida y cabeceras de IP real en `nginx.conf`:**
     - `client_max_body_size 6M;` (frena en el proxy cualquier intento de subida mayor al límite de 5 MB antes de impactar en Kestrel).
     - `proxy_set_header X-Real-IP $remote_addr;` y `proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;` y `proxy_set_header X-Forwarded-Proto $scheme;` (conecta con `UseForwardedHeaders` de `.NET 10`).
3. `docker-compose.yml`:
   - Orquesta `db` (`postgres:16-alpine`), `api` (`.NET 10` con volúmenes persistentes montados en `/var/www/documentos/` y `/var/www/backups/`) y `web` (`Nginx` en puertos 80/443).
4. **Pipeline de Integración Continua (`.github/workflows/ci.yml` — `RNF 10`):**
   - Ejecuta en cada push/PR: `dotnet build`, `dotnet test` (corriendo `LegajoMinero.Tests`) y `npm ci && npm run build` en `frontend/` para garantizar que nunca se despliegue código roto en el VPS de DonWeb.
5. **Prueba Verificable de Rendimiento `< 3 segundos` (`RNF 03`):**
   - Incluir en `LegajoMinero.Tests` (o script de benchmark `scripts/test-performance.ps1`) una prueba automatizada que siembre **500 trabajadores con 2.000 documentos** y mida el tiempo de respuesta de `GET /api/admin/trabajadores?page=1&pageSize=25`, `GET /api/usuarios/mi-dashboard` y `GET /api/verificacion/qr/{token}`, asertando que todos respondan en **`< 3000 ms`** (objetivo real `< 200 ms`).

---

## 4. Cierre de la Documentación de Tesis (Sección C)
Una vez verificado el funcionamiento completo de las Partes 1 a 5:
- [ ] Tomar capturas reales de las pantallas (Dashboard Trabajador, Subida de Documentos, QR con reloj anti-captura, Vista de Garita QR/DNI y Panel de RRHH).
- [ ] Completar en [TFT_Entrega_2_Actualizado.docx](file:///C:/Users/juan-dev/Desktop/PF/TFT_Entrega_2_Actualizado.docx):
  - `AGRADECIMIENTOS`
  - `PARTE 3 | DESARROLLO E IMPLEMENTACIÓN`
  - `PARTE 4 | MANUAL DE USUARIO` (paso a paso del Trabajador, de Garita y de RRHH con las capturas reales)
  - `PARTE 5 | CONCLUSIONES`
  - Limpieza final de formato y etiquetas `[reference:X]` en `BIBLIOGRAFÍA`.
