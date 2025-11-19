# Kanban Backend

## Configuración de la base de datos y JWT
1. Edita `src/Api/appsettings.json` o usa variables de entorno para definir `ConnectionStrings:Default`, `Database:Provider` (`postgres` o `sqlserver`) y la sección `Jwt` (`Issuer`, `Audience`, `Key`, `ExpirationInSeconds`).
2. Para desarrollo local con PostgreSQL, crea la BD `kanban` y asegúrate de que las credenciales coincidan.
3. En Docker o despliegues CI/CD usa variables de entorno (`ConnectionStrings__Default`, `Jwt__Key`, etc.).

## Migraciones EF Core
1. Instala las herramientas EF: `dotnet tool install --global dotnet-ef`.
2. Desde la carpeta `Backend`, crea la migración inicial:
   ```bash
   dotnet ef migrations add Init --project src/Infrastructure/Infrastructure.csproj --startup-project src/Api/Api.csproj --output-dir Data/Migrations
   ```
3. Aplica la migración:
   ```bash
   dotnet ef database update --project src/Infrastructure/Infrastructure.csproj --startup-project src/Api/Api.csproj
   ```

## Ejecución en desarrollo
1. Restaura dependencias: `dotnet restore KanbanBoard.sln`.
2. Ejecuta la API: `dotnet run --project src/Api/Api.csproj`.
3. Swagger estará en `https://localhost:7185/swagger` e incluye la documentación completa.
4. CORS permite `http://localhost:4200` para el frontend Angular.

## Probar login y flujo Kanban
1. Tras el primer arranque se ejecuta el *seed* que crea `admin@example.com / Pass123$` y un tablero demo.
2. Haz `POST /api/v1/auth/login` con esas credenciales para obtener el JWT (`accessToken`).
3. Usa el token (encabezado `Authorization: Bearer <token>`) en:
   - `GET /api/v1/boards` para listar tableros.
   - `GET /api/v1/boards/{boardId}` para traer columnas y tarjetas.
   - `POST /api/v1/boards/{boardId}/cards` para crear tarjetas y `PATCH /api/v1/boards/{boardId}/cards/move` para drag-and-drop enviando `prevPos/nextPos`.
   - `GET /api/v1/boards/{boardId}/export/excel` o `/pdf` para descargar el tablero.

## Docker
1. Construye la imagen: `docker build -t kanban-api .`.
2. Ejecuta sólo el backend: `docker run -p 8080:8080 kanban-api` (define las variables `ConnectionStrings__Default`, `Jwt__*`).
3. Con `docker-compose` levanta API + PostgreSQL:
   ```bash
   docker-compose up -d --build
   ```
4. La API quedará disponible en `http://localhost:8080/swagger` conectada al contenedor `db`.
