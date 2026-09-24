-- =============================================
-- LEGAJO DIGITAL MINERO - Base de Datos
-- Motor: PostgreSQL
-- Fecha: 2026-09-24
-- Autor: Juan José Sanchez García
-- =============================================

-- Eliminar tablas si existen (orden inverso por dependencias)
DROP TABLE IF EXISTS backup CASCADE;
DROP TABLE IF EXISTS sesion CASCADE;
DROP TABLE IF EXISTS verificacion CASCADE;
DROP TABLE IF EXISTS alerta CASCADE;
DROP TABLE IF EXISTS documento CASCADE;
DROP TABLE IF EXISTS tipo_documento CASCADE;
DROP TABLE IF EXISTS usuario CASCADE;

-- =============================================
-- TABLA 1: USUARIO
-- Representa al trabajador minero registrado
-- =============================================
CREATE TABLE usuario (
    id_usuario          SERIAL PRIMARY KEY,
    nombre_completo     VARCHAR(100) NOT NULL,
    dni                 VARCHAR(20) NOT NULL UNIQUE,
    email               VARCHAR(100) NOT NULL UNIQUE,
    telefono            VARCHAR(20),
    especialidad        VARCHAR(50),
    foto_perfil_url     TEXT,
    contrasena_hash     VARCHAR(255) NOT NULL,
    intentos_fallidos   INT DEFAULT 0,
    bloqueado_hasta     TIMESTAMP,
    fecha_registro      TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ultimo_acceso       TIMESTAMP,
    activo              BOOLEAN DEFAULT TRUE
);

-- =============================================
-- TABLA 2: TIPO_DOCUMENTO
-- Catálogo de tipos de documentos permitidos
-- =============================================
CREATE TABLE tipo_documento (
    id_tipo_documento   SERIAL PRIMARY KEY,
    nombre              VARCHAR(50) NOT NULL,
    descripcion         VARCHAR(255),
    obligatorio         BOOLEAN DEFAULT FALSE,
    dias_aviso          INT DEFAULT 15,
    activo              BOOLEAN DEFAULT TRUE
);

-- Datos iniciales del catálogo
INSERT INTO tipo_documento (nombre, obligatorio, dias_aviso) VALUES
    ('Examen Médico', TRUE, 15),
    ('Carnet de Conducir Alta Montaña', TRUE, 15),
    ('Curso de Seguridad e Higiene', TRUE, 15),
    ('Inducción General', TRUE, 15),
    ('Certificado de Altura', FALSE, 15),
    ('Título Profesional', FALSE, 30);

-- =============================================
-- TABLA 3: DOCUMENTO
-- Certificados y archivos del trabajador
-- =============================================
CREATE TABLE documento (
    id_documento        SERIAL PRIMARY KEY,
    id_usuario          INT NOT NULL REFERENCES usuario(id_usuario) ON DELETE CASCADE,
    id_tipo_documento   INT NOT NULL REFERENCES tipo_documento(id_tipo_documento),
    nombre_archivo      VARCHAR(255) NOT NULL,
    url_archivo         TEXT NOT NULL,
    descripcion         TEXT,
    fecha_vencimiento   DATE,
    tiene_vencimiento   BOOLEAN DEFAULT TRUE,
    fecha_carga         TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    fecha_actualizacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    es_obligatorio      BOOLEAN DEFAULT FALSE,
    estado              VARCHAR(20) DEFAULT 'VIGENTE' 
                        CHECK (estado IN ('VIGENTE', 'PROXIMO', 'VENCIDO'))
);

-- Índice para consultas frecuentes de vencimiento
CREATE INDEX idx_documento_vencimiento ON documento(id_usuario, fecha_vencimiento);
CREATE INDEX idx_documento_estado ON documento(id_usuario, estado);

-- =============================================
-- TABLA 4: ALERTA
-- Notificaciones enviadas al trabajador
-- =============================================
CREATE TABLE alerta (
    id_alerta           SERIAL PRIMARY KEY,
    id_usuario          INT NOT NULL REFERENCES usuario(id_usuario) ON DELETE CASCADE,
    id_documento        INT NOT NULL REFERENCES documento(id_documento) ON DELETE CASCADE,
    tipo_alerta         VARCHAR(20) NOT NULL 
                        CHECK (tipo_alerta IN ('EMAIL', 'DASHBOARD')),
    mensaje             TEXT NOT NULL,
    fecha_envio         TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    fecha_leida         TIMESTAMP,
    enviada             BOOLEAN DEFAULT FALSE
);

-- =============================================
-- TABLA 5: VERIFICACION
-- Escaneos QR del personal autorizado
-- =============================================
CREATE TABLE verificacion (
    id_verificacion     SERIAL PRIMARY KEY,
    id_usuario          INT NOT NULL REFERENCES usuario(id_usuario) ON DELETE CASCADE,
    ip_verificador      VARCHAR(45),
    estado_verificado   VARCHAR(20) NOT NULL 
                        CHECK (estado_verificado IN ('VERDE', 'AMARILLO', 'ROJO')),
    fecha_verificacion  TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =============================================
-- TABLA 6: SESION
-- Registro de inicios de sesión
-- =============================================
CREATE TABLE sesion (
    id_sesion           SERIAL PRIMARY KEY,
    id_usuario          INT NOT NULL REFERENCES usuario(id_usuario) ON DELETE CASCADE,
    token_sesion        VARCHAR(255) NOT NULL,
    ip_acceso           VARCHAR(45),
    dispositivo         VARCHAR(255),
    fecha_inicio        TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    fecha_cierre        TIMESTAMP,
    activa              BOOLEAN DEFAULT TRUE
);

-- =============================================
-- TABLA 7: BACKUP
-- Copias de seguridad del sistema
-- =============================================
CREATE TABLE backup (
    id_backup           SERIAL PRIMARY KEY,
    fecha_backup        TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    tamano_mb           DECIMAL(10,2),
    ubicacion_url       TEXT,
    tipo_backup         VARCHAR(20) DEFAULT 'COMPLETO' 
                        CHECK (tipo_backup IN ('COMPLETO', 'INCREMENTAL')),
    exitoso             BOOLEAN DEFAULT TRUE
);

-- =============================================
-- NOTAS DE DISEÑO (Ajustes MVP)
-- =============================================
-- 1. tipo_alerta: Se eliminó 'PUSH' del CHECK constraint.
--    Para el MVP solo se usan alertas EMAIL y DASHBOARD.
--    Las Push Notifications se implementarán en una fase futura.
--
-- 2. Los índices en documento optimizan las consultas
--    del semáforo (filtrar por usuario + estado/vencimiento).
--
-- 3. El campo estado en documento se calcula dinámicamente
--    en el backend comparando fecha_vencimiento con la fecha actual.
--    El valor almacenado sirve como caché para consultas rápidas.
-- =============================================
