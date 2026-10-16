# Gestión de ventas: módulo de usuarios

Proyecto inicial en **C# + Windows Forms + MySQL** para integrar al trabajo grupal. Incluye inicio de sesión, creación del primer administrador, consulta/búsqueda, alta, modificación, cambio de contraseña, desactivación y roles.

## Qué hace cada rol

- **Administrador:** puede administrar usuarios.
- **Supervisor y Vendedor:** pueden iniciar sesión, pero no ven ni usan la administración de usuarios.
- La pantalla de ventas se integra en el espacio indicado en `MainForm.cs` cuando el grupo construya esa parte.

## Preparar la base de datos

1. Instalá MySQL Server y abrí MySQL Workbench.
2. Ejecutá el script `gestion_ventas.sql`. Crea la base `gestion_ventas`.
3. En Windows, definí la variable de entorno `GESTION_VENTAS_CONNECTION` con tu cadena local. Ejemplo en PowerShell (reemplazá `TU_CLAVE`):

   ```powershell
   [Environment]::SetEnvironmentVariable('GESTION_VENTAS_CONNECTION', 'Server=localhost;Port=3306;Database=gestion_ventas;User ID=root;Password=TU_CLAVE;', 'User')
   ```

4. Cerrá y volvé a abrir Visual Studio para que lea la variable.
5. Abrí `GestionVentas.sln` con Visual Studio 2022 (con soporte para .NET 8) o más reciente. Restaurá los paquetes NuGet y ejecutá con **F5**.
6. En el primer inicio, la aplicación crea la tabla `usuarios` y te pide crear el primer administrador. Después iniciás sesión con ese usuario.

La aplicación guarda contraseñas como hash PBKDF2 con salt aleatorio. No subas tu cadena real de conexión ni contraseñas a GitHub.

## Archivos principales

- `LoginForm.cs`: acceso al sistema y configuración inicial del administrador.
- `MainForm.cs`: pantalla principal y administración visual de cuentas.
- `UserRepository.cs`: consultas SQL parametrizadas para leer y guardar usuarios.
- `PasswordHasher.cs`: creación y verificación de hashes de contraseña.
- `Database.cs`: conexión a MySQL y creación de la tabla.
- `gestion_ventas.sql`: creación de la base.

## Para integrarlo con el proyecto del grupo

Este es un módulo de usuarios para incorporar al proyecto total. Cuando el grupo tenga una solución existente, pueden agregar estos archivos al proyecto WinForms, instalar el paquete NuGet `MySqlConnector` y conectar `MainForm` con la pantalla principal de ventas. La autorización de la administración se concentra en `UserPermissions.CanManageUsers`.

## Rama de Git

Copiá la carpeta `GestionVentas` dentro del repositorio del grupo y desde la terminal de Visual Studio, parado en la carpeta del repositorio, creá tu rama:

```bash
git switch -c feature/administracion-usuarios
git add GestionVentas
git commit -m "Agregar módulo de administración de usuarios"
git push -u origin feature/administracion-usuarios
```

Después abrí un Pull Request para que tus amigos revisen e integren el cambio.
