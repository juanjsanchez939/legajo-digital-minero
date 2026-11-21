# Detiene el PostgreSQL portable de desarrollo.
$ErrorActionPreference = 'Stop'
$root = Join-Path $env:LOCALAPPDATA 'LegajoMinero'
$pg = Join-Path $root 'pgsql'
$data = Join-Path $root 'data'

if (-not (Test-Path (Join-Path $data 'PG_VERSION'))) {
    throw 'No existe el cluster local. Ejecutá scripts/setup-local-db.ps1 primero.'
}

& (Join-Path $pg 'bin\pg_ctl.exe') -D $data stop -m fast
