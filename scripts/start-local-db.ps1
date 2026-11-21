# Inicia el PostgreSQL portable de desarrollo.
# Si ya hay un servidor escuchando en 5433 (por ejemplo, el servicio de Windows
# postgresql-x64-16), no hace nada.
$ErrorActionPreference = 'Stop'

$port = 5433
$root = Join-Path $env:LOCALAPPDATA 'LegajoMinero'
$pg = Join-Path $root 'pgsql'
$data = Join-Path $root 'data'

function Test-TcpPort([int]$Port) {
    $client = New-Object System.Net.Sockets.TcpClient
    try {
        $client.Connect('127.0.0.1', $Port)
        return $true
    }
    catch {
        return $false
    }
    finally {
        $client.Dispose()
    }
}

if (Test-TcpPort $port) {
    Write-Host "Ya hay un PostgreSQL escuchando en el puerto $port."
    exit 0
}

if (-not (Test-Path (Join-Path $data 'PG_VERSION'))) {
    throw 'No existe el cluster portable. Ejecutá scripts/setup-local-db.ps1 primero.'
}

& (Join-Path $pg 'bin\pg_ctl.exe') -D $data -l (Join-Path $root 'postgres.log') -o "-p $port" start
