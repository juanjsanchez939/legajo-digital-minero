namespace LegajoMinero.Api.Domain.Enums;

public enum RolUsuario
{
    TRABAJADOR,
    ADMINISTRADOR
}

public enum EstadoDocumento
{
    VIGENTE,
    PROXIMO,
    VENCIDO
}

public enum EstadoRevision
{
    PENDIENTE,
    VALIDADO,
    OBSERVADO
}

public enum TipoAlerta
{
    PUSH,
    EMAIL,
    DASHBOARD
}

public enum OrigenAlerta
{
    AUTOMATICA,
    MANUAL_RRHH
}

public enum MetodoVerificacion
{
    QR,
    DNI,
    PANEL_RRHH
}

public enum EstadoVerificado
{
    VERDE,
    AMARILLO,
    ROJO
}

public enum TipoBackup
{
    COMPLETO,
    INCREMENTAL
}
