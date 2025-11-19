# Kanban Frontend (Angular 20)

SPA en Angular que consume el backend .NET del tablero Kanban. Incluye autenticación con JWT, tablero dinámico con drag & drop y descargas en PDF/Excel.

## Requisitos previos
- Node.js 20.19 o superior (recomendado por Angular 20).
- Backend corriendo en `http://localhost:5031` (o actualiza `src/environments/environment*.ts`).

## Configuración del entorno
1. Instala dependencias:
   ```bash
   npm install
   ```
2. Verifica/ajusta la URL del backend en:
   - `src/environments/environment.development.ts` (modo `ng serve`).
   - `src/environments/environment.ts` (build producción).

## Ejecutar en desarrollo
```bash
npm start
# ó
ng serve
```
La app correrá en `http://localhost:4200`. Usa el usuario semilla `admin@example.com / Pass123$` para iniciar sesión, obtener un JWT y acceder al tablero.

## Funcionalidades principales
- **Login**: formulario reactivo, almacenamiento del token en `localStorage`, refresco automático al expirar.
- **Gestión de tableros**: listado lateral, creación rápida y selección automática.
- **Tablero Kanban**:
  - CRUD de columnas y tarjetas (formularios inline).
  - Drag & drop con Angular CDK; calcula `prevPos/nextPos` y llama a `/cards/move`.
  - Indicadores de estado/mensajes de error.
- **Exportaciones**: botones para bajar PDF o Excel vía `/export/{pdf|excel}`.
- **Seguridad**: `AuthGuard` bloquea `/dashboard`, interceptor agrega `Authorization: Bearer` y expulsa al usuario en 401.

## Pruebas manuales sugeridas
1. Inicia sesión.
2. Crea un tablero nuevo y agrega columnas "To Do", "In Progress", "Done".
3. Crea tarjetas dentro de cada columna, arrástralas entre columnas y observa que el backend mantiene el orden.
4. Descarga el PDF/Excel para validar el contenido.

## Build de producción
```bash
ng build --configuration production
```
Los artefactos se generan en `dist/student-dashboard/browser`. Sirve esa carpeta detrás de cualquier CDN o reverse proxy apuntando al backend en `/api/v1`.
