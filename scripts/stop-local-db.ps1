# Detiene el PostgreSQL portable de desarrollo.
# No detiene el servicio de Windows postgresql-x64-16.
$ErrorActionPreference = 'Stop'

$root = Join-Path $env:LOCALAPPDATA 'LegajoMinero'
$pg = Join-Path $root 'pgsql'
$data = Join-Path $root 'data'

if (-not (Test-Path (Join-Path $data 'PG_VERSION'))) {
    Write-Host 'No existe el cluster portable; no hay nada que detener.'
    exit 0
}

& (Join-Path $pg 'bin\pg_ctl.exe') -D $data status *> $null
if ($LASTEXITCODE -ne 0) {
    Write-Host 'El cluster portable no está corriendo (el servicio de Windows no se toca).'
    exit 0
}

& (Join-Path $pg 'bin\pg_ctl.exe') -D $data stop -m fast
