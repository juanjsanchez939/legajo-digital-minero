# Legajo Digital Minero — "Legajo en el bolsillo"

Billetera digital de acreditación minera para trabajadores de San Juan, Argentina.
Centraliza certificados, controla vencimientos y permite verificar la aptitud con
un código QR dinámico, incluso sin señal en alta montaña.

> **Estado:** Día 1 de 15 — esqueleto del monorepo, API y PWA base.
> La documentación funcional está en [`PLAN_MAESTRO.md`](./PLAN_MAESTRO.md) y
> [`specs/`](./specs/).

## Stack

| Capa | Tecnología |
|---|---|
| Frontend | React 19 + Vite + TypeScript + Tailwind CSS (PWA) |
| Backend | ASP.NET Core 10 Web API + EF Core 10 |
| Base de datos | PostgreSQL 16 |
| Almacenamiento | Disco del VPS (`/var/www/documentos/`) |
| Infraestructura | DonWeb VPS + Nginx + Docker Compose |

## Estructura

```
.
├─ backend/
│  ├─ LegajoMinero.sln
│  ├─ LegajoMinero.Api/        # Domain / Application / Infrastructure / Controllers
│  └─ LegajoMinero.Tests/      # xUnit
├─ frontend/                   # PWA React + Vite
├─ deploy/                     # Docker, Nginx y compose (Parte 5)
├─ docs/                       # Diagramas y capturas
├─ scripts/                    # Utilidades de desarrollo (PostgreSQL local)
├─ specs/                      # Especificación de las 5 partes
├─ PLAN_MAESTRO.md             # Plan maestro y decisiones
└─ prototipo.png               # Referencia visual del dashboard
```

## Requisitos

- .NET SDK 10 (`dotnet --version`)
- Node.js 22+ y npm
- PostgreSQL 16 (o el script portable de `scripts/`)

## Puesta en marcha

### 1. Base de datos

En desarrollo se usa un PostgreSQL portable dentro del perfil del usuario, sin
permisos de administrador:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/setup-local-db.ps1
```

Luego, para iniciar o detener el servidor:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/start-local-db.ps1
powershell -ExecutionPolicy Bypass -File scripts/stop-local-db.ps1
```

La base se llama `legajo_minero`, usuario `postgres`, puerto `5432`.

### 2. Backend

Copiá `backend/LegajoMinero.Api/appsettings.Development.example.json` a
`appsettings.Development.json` y ajustá las claves locales. El archivo real está
ignorado por Git.

```powershell
cd backend
dotnet run --project LegajoMinero.Api
```

- API: <http://localhost:5235>
- Health: <http://localhost:5235/health>
- OpenAPI: <http://localhost:5235/openapi/v1.json>
- Scalar UI: <http://localhost:5235/scalar/v1>

### 3. Frontend

```powershell
cd frontend
npm install
npm run dev
```

La PWA queda en <http://localhost:5173> y el proxy `/api` apunta al backend.

### 4. Pruebas

```powershell
cd backend
dotnet test
```

```powershell
cd frontend
npm run build
```

## Alcance acordado

- **Una empresa por instalación**: el DER de la tesis tiene 7 entidades y no
  incluye la entidad `empresa`.
- Alertas "push" implementadas como avisos in-app + correo electrónico, sin
  Web Push/FCM externo.
- Backups con `pg_dump` real y copia descargable fuera del VPS.

## Roadmap

| Día | Entregable |
|---|---|
| 1 | Repo, PostgreSQL, API base, PWA shell, CI |
| 2 | 7 entidades, AES-256, migración y seeder |
| 3–4 | Autenticación, perfil y sesiones |
| 5–7 | Documentos, semáforo, QR y garita |
| 8–10 | Panel RRHH, alertas y backups |
| 11–13 | PWA del trabajador y modo offline |
| 14–15 | Panel RRHH completo, despliegue y smoke test |

## Documentación

- [Plan maestro](./PLAN_MAESTRO.md)
- [Parte 1 — Base de datos y auth](./specs/PARTE_1_BASE_DATOS_Y_AUTH.md)
- [Parte 2 — Documentos, semáforo y garita](./specs/PARTE_2_DOCUMENTOS_SEMAFORO_Y_GARITA.md)
- [Parte 3 — RRHH y alertas](./specs/PARTE_3_MODULO_RRHH_Y_ALERTAS.md)
- [Parte 4 — PWA del trabajador y offline](./specs/PARTE_4_FRONTEND_PWA_TRABAJADOR_Y_OFFLINE.md)
- [Parte 5 — Panel RRHH, despliegue y cierre](./specs/PARTE_5_PANEL_RRHH_DEPLOY_DONWEB_Y_CIERRE.md)
- `TFT_Entrega_1.docx` y `TFT_Entrega_2_Actualizado.docx`: documento de tesis
  (no versionados, se conservan localmente).
