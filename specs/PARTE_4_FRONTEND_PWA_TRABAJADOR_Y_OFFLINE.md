# PARTE 4: Frontend PWA del Trabajador, Modo Offline y Vista de Garita

> ⚠️ **REVISAR DOCUMENTACIÓN PARA IMPLEMENTACIÓN:**  
> - **Referencia visual obligatoria:** Revisar [prototipo.png](file:///C:/Users/juan-dev/Desktop/PF/prototipo.png) para respetar la estética general (encabezado azul marino oscuro `#0f2e5a`, foto y nombre del trabajador, tarjeta central **"MI ESTADO DE APTITUD"** con ícono circular grande del semáforo, lista **"RESUMEN DOCUMENTAL"** con barras laterales de color y navegación inferior de 4 pestañas: `Dashboard`, `Documentos`, `Alertas`, `QR`).  
> - Revisar **"HISTORIAS DE USUARIO (HU-01 a HU-14 y HU-18)"** en [TFT_Entrega_2_Actualizado.docx](file:///C:/Users/juan-dev/Desktop/PF/TFT_Entrega_2_Actualizado.docx).

---

## 1. Objetivo de esta Parte
Desarrollar la Aplicación Web Progresiva (**PWA**) en **React + Vite + TypeScript** (`frontend/`) con soporte completo para el trabajador minero, **Modo Offline real** en alta montaña (`ServiceWorker` + `IndexedDB`) y la **Vista Pública de Garita** (verificación por escaneo de QR y búsqueda alternativa por DNI).

---

## 2. Arquitectura del Frontend (`frontend/`)

- **Stack:** `React + Vite + TypeScript` + `React Router` + `Lucide Icons` + generador QR en cliente (`qrcode.react` o canvas SVG nativo para que el QR se renderice incluso 100% offline).
- **Configuración PWA Instalable (`manifest.webmanifest` + `ServiceWorker` — `RNF 04`, `RNF 09`):**
  - Configurar `manifest.webmanifest` con `name: "Legajo en el bolsillo"`, `short_name: "LegajoMinero"`, `display: "standalone"`, `theme_color: "#0f2e5a"`, `background_color: "#ffffff"`, orientación portrait e íconos `192x192` y `512x512`.
  - Incluir botón/banner **"Instalar App"** (`beforeinstallprompt`) y precacheo de todos los bundles HTML/CSS/JS para que la aplicación abra instantáneamente en la cordillera sin señal.
- **Almacenamiento Offline (`src/services/offlineDb.ts`):**
  - Implementar con `IndexedDB` un store local `legajo_offline_store` que guarde tras cada sincronización exitosa:
    - Perfil del trabajador (`nombreCompleto`, `dni`, `especialidad`, `fotoPerfilUrl`, `qrToken`, `activo`).
    - Lista de documentos cargados y catálogo de `tipos_documento`.
    - blobs/base64 opcionales de los certificados visualizados recientemente.
    - `ultimaSincronizacionUtc`.
- **Confirmación previa al guardado (`HU-05`, `HU-06` y Contingencia `C-12`):**
  - En el modal de carga/actualización de documentos, mostrar un resumen de confirmación antes de guardar (*Tipo de documento seleccionado, archivo y fecha de vencimiento*) para minimizar el error humano al cargar certificados (`C-12`).
- **Recálculo Local del Semáforo y Guard Offline con Token Vencido (`HU-11`):**
  - **Regla crítica del `AuthGuard` Offline:** Si el Access Token JWT (30 min) expiró mientras el trabajador sube a la cordillera sin señal (`navigator.onLine == false` o error de red al intentar `POST /api/auth/refresh`), el `AuthGuard` **NO debe redirigir al Login ni borrar la sesión local**. Si existe un snapshot válido del usuario en `IndexedDB` (`legajo_offline_store`), la PWA entra automáticamente en **Modo Offline de Solo Lectura**, permitiendo visualizar su Dashboard, Semáforo, Documentos cacheados y Código QR. Solo cuando recupere señal intentará renovar el token en segundo plano.
  - Además, en modo offline la PWA lee el snapshot de `IndexedDB` y **recalcula en vivo** el estado de cada documento comparando `fecha_vencimiento` contra la fecha actual en `America/Argentina/San_Juan`. Si un documento venció hoy estando sin señal, el semáforo en pantalla pasa a **ROJO** localmente.

---

## 3. Pantallas del Trabajador (Diseño Mobile-First basado en `prototipo.png`)

### 3.1. Autenticación (`Login`, `Registro`, `Recuperar Contraseña` — `HU-01`, `HU-02`, `HU-03`)
- Pantalla limpia con identidad visual minera (azul petróleo `#0f2e5a` y acentos de alta visibilidad).
- En **Registro (`HU-01`)**, incluir **checkbox obligatorio** de consentimiento: *"Acepto el tratamiento de mis datos personales y médicos conforme a la Ley N° 25.326 de Protección de Datos Personales"*, que envía `aceptaConsentimientoDatos: true` al backend.
- Muestra claramente los mensajes de error y el contador de bloqueo temporal de 15 minutos si se alcanzan los 5 intentos fallidos.

### 3.2. Pestaña 1: `Dashboard` (`HU-07`, `HU-14`)
- **Header superior:** Foto de perfil, nombre del trabajador, especialidad, indicador de estado de conexión (`🟢 En línea` / `🟠 Modo Offline — Últ. sinc: HH:mm`) y acceso a `Mi Perfil`.
- **Reloj en vivo anti-captura:** Barra discreta con fecha y hora actual corriendo con segundos en tiempo real (`dd/MM/yyyy HH:mm:ss`).
- **Tarjeta central "MI ESTADO DE APTITUD":**
  - Círculo luminoso grande:
    - 🟢 **APTO — VIGENTE** (Verde)
    - 🟡 **ATENCIÓN — PRÓXIMO A VENCER / EN REVISIÓN** (Amarillo)
    - 🔴 **NO APTO — DOCUMENTACIÓN VENCIDA / FALTANTE / OBSERVADA** (Rojo)
  - Contadores rápidos: `Vigentes`, `Próximos / En revisión`, `Vencidos / Faltantes`.
  - **Indicador de última sincronización (`HU-14`):** Muestra claramente la fecha y hora exacta de la última sincronización con el servidor (`Última sincronización: dd/MM/yyyy HH:mm`), especialmente visible cuando está en Modo Offline.
  - **Botón de acción principal (`HU-14`):** Botón destacado **"Cargar nuevo documento"** en el dashboard para acceso inmediato.
- **Listas de Atención Prioritaria del Dashboard (`HU-14`):**
  - **Lista "Próximos a vencer":** Muestra **hasta un máximo de 5 documentos** próximos a vencer (o pendientes de revisión), ordenados por fecha más cercana, cada uno con enlace directo para actualizarlo rápidamente.
  - **Lista "Documentos Vencidos / Observados":** Si existen documentos vencidos, faltantes obligatorios u observados por RRHH, se muestran destacados con fondo/borde **rojo** y acceso directo para renovarlos.
- **Sección "RESUMEN DOCUMENTAL":**
  - Lista de cada tipo de documento obligatorio y opcional con barra vertical de color (`Verde`, `Amarillo`, `Rojo`), fecha de vencimiento (`VIGENTE 31/12/2026`, `PRÓXIMO A VENCER`, `OBSERVADO POR RRHH: <motivo>`, o `FALTANTE`) y badge de estado en español (`Vigente`, `En revisión`, `Próximo`, `Vencido`, `Observado`, `Faltante`).
  - Botón directo para cargar o actualizar cualquier documento desde la misma fila.

### 3.3. Pestaña 2: `Documentos` (`HU-05`, `HU-06`, `HU-12`, `HU-13`)
- Listado completo de documentos del trabajador con botón **"Cargar / Renovar Documento"**.
- Modal/formulario de carga:
  - Selector de `Tipo de Documento`.
  - Checkbox `"Documento sin fecha de vencimiento"` (si no está marcado, selector de fecha obligatorio con validación **`fecha >= hoy`**, alineado con `PARTE_2`).
  - Input de archivo con validación en cliente antes de subir: solo `.jpg`, `.jpeg`, `.png`, `.pdf` y tamaño `<= 5 MB`.
  - Visor integrado para abrir el PDF o imagen cargada y botón de eliminación (solo visible si el documento no es obligatorio).

### 3.4. Pestaña 3: `Alertas` (`HU-08`)
- Bandeja de notificaciones (tanto las automáticas de `15`, `7`, `3` y `0` días como los avisos manuales enviados por RRHH).
- Badge numérico en la barra inferior con la cantidad de alertas no leídas y botón `"Marcar todas como leídas"`.

### 3.5. Pestaña 4: `QR` y `Mi Perfil` (`HU-04`, `HU-09`, `HU-11`)
- Muestra el **Código QR Dinámico** grande y de alto contraste (generado localmente a partir de `/verificacion/{qr_token}` para que funcione 100% offline), junto con el **reloj en vivo con segundos**, el DNI del trabajador y botón para descargar/compartir el QR.
- Vista de **Mi Perfil** (`HU-04`): edición de nombre, teléfono, especialidad, foto de perfil, switch de `"Recibir alertas por correo electrónico"` y cambio de contraseña (el DNI se muestra bloqueado como solo lectura).

---

## 4. Vista Pública de Garita (`/verificacion/:qrToken` y `/garita` — `HU-10`, `HU-18`)
- **No requiere iniciar sesión.**
- Al escanear el QR (`/verificacion/:qrToken`) o al ingresar el número de DNI en el buscador de contingencia (`/garita` — Contingencia `C-08`):
  - Muestra instantáneamente la credencial de verificación con foto, nombre completo, **DNI enmascarado (`dniEnmascarado`, ej. `**.***.456`)**, especialidad, **Semáforo General gigante** (`APTO` / `ATENCIÓN` / `NO APTO` o `TRABAJADOR INACTIVO`), timestamp exacto de la consulta y la tabla con el estado de cada documento **sin exponer nunca los archivos PDF/imágenes originales**.

---

## 5. Checklist de Verificación (Definition of Done — Parte 4)
- [ ] El diseño del Dashboard del trabajador respeta la estructura visual de [prototipo.png](file:///C:/Users/juan-dev/Desktop/PF/prototipo.png) con etiquetas reales en español.
- [ ] La validación en cliente rechaza archivos `> 5 MB` o formatos no permitidos antes de enviarlos al servidor.
- [ ] Al desconectar internet (Modo Offline en DevTools), el trabajador sigue viendo su perfil, su semáforo recalculado localmente y su código QR con el reloj en vivo.
- [ ] La vista pública de Garita permite verificar tanto por link de QR como buscando por DNI sin exponer los certificados originales.
