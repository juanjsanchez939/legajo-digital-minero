# 👷🏽‍♂️ Legajo Digital Minero — "Legajo en el Bolsillo"

> Billetera digital de acreditación minera para trabajadores del sector minero de San Juan, Argentina.

[![Status](https://img.shields.io/badge/status-en%20desarrollo-yellow)]()
[![License](https://img.shields.io/badge/license-MIT-blue)]()

---

## 📋 Descripción

**Legajo en el Bolsillo** es una Aplicación Web Progresiva (PWA) que permite a los trabajadores mineros centralizar toda su documentación obligatoria (exámenes médicos, carnets de conducir en alta montaña, cursos de seguridad, inducciones) en un único perfil digital verificable mediante código QR.

### Problema que resuelve

En la industria minera de San Juan, la documentación se gestiona de forma dispersa: carpetas físicas que se deterioran, fotos en el celular, PDFs por WhatsApp. Esto provoca que los trabajadores lleguen a la garita con documentación vencida, generando pérdidas de tiempo y productividad.

### Solución

- 📂 **Centralización:** Un único repositorio digital para todos los certificados
- 🚦 **Semáforo de Aptitud:** Indicador visual (Verde/Amarillo/Rojo) del estado documental
- 🔔 **Alertas Preventivas:** Notificaciones a los 15, 7, 3 días y el día del vencimiento
- 📱 **Código QR Dinámico:** Verificación instantánea en garita sin revisar carpetas físicas
- 📶 **Modo Offline:** Acceso a datos básicos y QR en zonas sin conectividad

---

## 🛠️ Stack Tecnológico

| Capa               | Tecnología                     |
| ------------------- | ------------------------------ |
| **Frontend**        | React (PWA) + TailwindCSS      |
| **Backend**         | .NET 8 (API REST con C#)       |
| **Base de Datos**   | PostgreSQL                     |
| **Almacenamiento**  | Cloudinary / Azure Blob Storage|
| **Autenticación**   | JWT (JSON Web Tokens)          |
| **QR**              | QRCoder (.NET)                 |

---

## 📁 Estructura del Proyecto

```
legajo-digital-minero/
├── frontend/          # Aplicación React (PWA)
├── backend/           # API REST en .NET 8
├── docs/              # Documentación del proyecto
│   ├── diagramas/     # Diagramas E-R, arquitectura, flujos
│   └── capturas/      # Capturas de pantalla de la app
├── scripts/           # Scripts útiles (setup de BD, seeds, etc.)
├── .gitignore
├── CHANGELOG.md       # Registro de cambios por día/sprint
└── README.md
```

---

## 🗄️ Modelo de Base de Datos

### Entidades Principales

| #  | Entidad          | Descripción                                              |
| -- | ---------------- | -------------------------------------------------------- |
| 1  | USUARIO          | Trabajador minero registrado en el sistema               |
| 2  | DOCUMENTO        | Certificado o archivo cargado por el trabajador          |
| 3  | TIPO_DOCUMENTO   | Catálogo de tipos (examen médico, carnet, curso, etc.)   |
| 4  | ALERTA           | Notificaciones enviadas al trabajador                    |
| 5  | VERIFICACION     | Registro de escaneos QR por personal autorizado          |
| 6  | SESION           | Registro de inicios de sesión                            |
| 7  | BACKUP           | Registro de copias de seguridad del sistema              |

---

## 🚦 Semáforo de Aptitud

| Color      | Significado                                                  |
| ---------- | ------------------------------------------------------------ |
| 🟢 Verde   | Todos los documentos vigentes                                |
| 🟡 Amarillo | Algún documento vence en los próximos 15 días               |
| 🔴 Rojo    | Algún documento vencido o falta documentación obligatoria    |

---

## 📅 Roadmap de Desarrollo

- [x] **Semana 1:** Backend base, BD PostgreSQL, Autenticación JWT
- [ ] **Semana 2:** Lógica de negocio (semáforo, QR, storage de archivos)
- [ ] **Semana 3:** Frontend React PWA (Dashboard, formularios, QR)
- [ ] **Semana 4:** Modo Offline, QA, despliegue y documentación final

---

## 👤 Autor

**Juan José Sanchez García**
Tecnicatura Universitaria en Desarrollo de Software
Facultad de Ciencias Económicas y Empresariales

**Tutores:** Cristian Fernández — Fernando Mas

---

## 📄 Licencia

Este proyecto es parte de un Trabajo Final de Título (TFT) con fines académicos.
