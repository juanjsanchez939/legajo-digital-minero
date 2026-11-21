# PostgreSQL local (desarrollo)
#
# Instala PostgreSQL 16 en modo portable dentro del perfil del usuario,
# sin requerir permisos de administrador, y crea la base legajo_minero.
#
# Uso:
#   powershell -ExecutionPolicy Bypass -File scripts/setup-local-db.ps1
#
# Para producción se usa el contenedor postgres:16-alpine de docker-compose.

$ErrorActionPreference = 'Stop'

$version = '16.16-2'
$root = Join-Path $env:LOCALAPPDATA 'LegajoMinero'
$pg = Join-Path $root 'pgsql'
$data = Join-Path $root 'data'
$zip = Join-Path $env:TEMP "postgresql-$version-binaries.zip"
$url = "https://get.enterprisedb.com/postgresql/postgresql-$version-windows-x64-binaries.zip"
$password = if ($env:LEGAJO_DB_PASSWORD) { $env:LEGAJO_DB_PASSWORD } else { 'postgres' }

if (-not (Test-Path (Join-Path $pg 'bin\pg_ctl.exe'))) {
    Write-Host "Descargando PostgreSQL $version..."
    if (-not (Test-Path $zip)) {
        curl.exe -L --retry 3 -o $zip $url
    }
    New-Item -ItemType Directory -Force -Path $root | Out-Null
    Write-Host 'Extrayendo...'
    tar.exe -xf $zip -C $root
}

if (-not (Test-Path (Join-Path $data 'PG_VERSION'))) {
    Write-Host 'Inicializando cluster...'
    $pwFile = Join-Path $env:TEMP 'legajo-pg-pw.txt'
    Set-Content -Path $pwFile -Value $password -NoNewline -Encoding ASCII
    try {
        & (Join-Path $pg 'bin\initdb.exe') -D $data -U postgres --pwfile=$pwFile `
            --auth-host=scram-sha-256 --auth-local=trust -E UTF8 --locale=C
        if ($LASTEXITCODE -ne 0) { throw 'initdb falló' }
    }
    finally {
        Remove-Item $pwFile -Force -ErrorAction SilentlyContinue
    }
}

Write-Host 'Iniciando servidor...'
& (Join-Path $pg 'bin\pg_ctl.exe') -D $data -l (Join-Path $root 'postgres.log') -o '-p 5432' start
Start-Sleep -Seconds 2

$env:PGPASSWORD = $password
& (Join-Path $pg 'bin\createdb.exe') -h localhost -p 5432 -U postgres legajo_minero 2>$null
Write-Host 'PostgreSQL listo en localhost:5432 (base: legajo_minero).'
