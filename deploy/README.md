# Despliegue

Esta carpeta se completa en la Parte 5 (Día 14). Va a contener:

- `backend.Dockerfile` — imagen multi-stage de .NET 10.
- `frontend.Dockerfile` — build de Vite + Nginx.
- `nginx.conf` — estáticos, proxy `/api`, límite de subida y headers de IP.
- `docker-compose.yml` — `db` (postgres:16-alpine), `api`, `web`.

Requisitos de producción: DonWeb Cloud Server (2 vCPU, 4 GB RAM, 50 GB SSD),
Nginx con SSL de Let's Encrypt y volúmenes persistentes para documentos y backups.
