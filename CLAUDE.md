# ThenorSound Backend

GitHub: `throwExceptions/thenorsound-backend`

## Architecture
.NET 10 microservices, each following Clean Architecture + CQRS:
```
Service/
├── Domain/          # Entities, Enums, Repository interfaces (IUserRepository)
├── Application/     # Commands/Queries (MediatR), Validators (FluentValidation), Client interfaces
├── Infra/           # MongoDB repos, HttpClient implementations, Settings classes
├── API/             # Controllers, Request/Response DTOs, Program.cs
├── API.Test/        # Handler + Validator tests (xUnit, Moq)
└── Infra.Test/      # Client tests (xUnit, Moq)
```
- **Mapping**: Mapster (NOT AutoMapper)
- **Database**: MongoDB via `MongoDB.Driver`, abstracted behind repository interfaces
- **Response envelope**: All endpoints return `BaseResponse<T>` with `{ result, success, error }`
- **Validation**: FluentValidation in API layer, business validation in command handlers

## Services

### Customer Service (`Customer/`)
- CRUD for customers (companies and crews)
- `CustomerType` enum: Customer=1, Crew=2, Admin=3
- Ports: dotnet run HTTP 5063 / HTTPS 7030, Docker 6063

### User Service (`User/`)
- CRUD for users belonging to customers
- `Role` enum: Superuser=1, Admin=2, User=3
- Calls Customer service to validate CustomerType matches UserType on create/update
- **Superuser (Role=1)** skips CustomerType validation, CustomerId optional
- Admin and User require CustomerId on create
- Ports: dotnet run HTTP 50572 / HTTPS 50571, Docker 6062

### Auth Service (`Auth/`)
- Issues and validates JWT access tokens (15 min, response body) and refresh tokens (7 days, HTTP-only cookie)
- Own MongoDB collection: `credentials` — stores userId, email, passwordHash, refreshToken, refreshTokenExpiry
- Calls User service (`GET /api/User/by-email/{email}`) to enrich JWT claims
- Refresh token cookie path restricted to `/api/Auth/refresh` (token rotation on every refresh)
- BCrypt (workFactor=12) for password hashing
- Ports: dotnet run HTTP 5264 / HTTPS 7264, Docker 6061
- User secrets required locally: `JwtSettings:SecretKey` (min 32 chars), `MongoDbSettings:ConnectionString`

#### CustomerId update rules (`UpdateUserCommandHandler`)
- `request.CustomerId == null` → field not sent, no change (all roles)
- `request.CustomerId == ""` → clear CustomerId, only applied if `user.Role == Superuser`
- `request.CustomerId == "someId"` → validate customer exists, only applied if `user.Role == Superuser`
- Admin/User: CustomerId block is always skipped regardless of what is sent

#### UserRepository.UpdateAsync
All fields are explicitly copied to `existingEntity` before `ReplaceOneAsync`.
`CustomerId` is included: `existingEntity.CustomerId = user.CustomerId ?? string.Empty`

### Service-to-Service Communication (User → Customer)
- Interface: `Application/Clients/ICustomerClient.cs`
- Implementation: `Infra/Clients/CustomerClient.cs` (typed HttpClient)
- Config: `Infra/Settings/CustomerClientConfiguration.cs`
- SSL bypass only in Development: `IHostEnvironment.IsDevelopment()` check in `Infrastructure.cs`
- Config override for Docker: `CustomerClientConfiguration__BaseUrl=http://customer-service:8080`

## Configuration Hierarchy
1. `appsettings.json` (defaults, points to localhost HTTPS for dotnet run)
2. `appsettings.Development.json` (dev overrides)
3. Environment variables (Docker/Azure overrides using `__` separator)

### Key appsettings (User)
```json
"CustomerClientConfiguration": {
    "BaseUrl": "https://localhost:7030",
    "GetByIdEndpoint": "api/Customer/{0}"
}
"MongoDbSettings": {
    "ConnectionString": "mongodb+srv://...",  // Azure Cosmos DB for dotnet run
    "DatabaseName": "thenorsound",
    "UserCollectionName": "users"
}
```

## Docker Compose
```bash
docker compose up --build -d    # Start all services
docker compose down             # Stop all
```
Ports: Customer=6063, User=6062, MongoDB=27017
Uses local MongoDB (not Azure Cosmos DB).

## Azure Deployment
Container Apps on Consumption plan, resource group: `rg-mongodb`
```bash
# Build, push, deploy
docker build -t ghcr.io/throwexceptions/thenorsound-<service>-api:latest ./<Service>
docker push ghcr.io/throwexceptions/thenorsound-<service>-api:latest
az containerapp update --name thenorsound-<service>-api --resource-group rg-mongodb --image ghcr.io/throwexceptions/thenorsound-<service>-api:latest
```
GHCR auth needs `write:packages` scope: `gh auth refresh -h github.com -s write:packages`

## Gotchas
- Need `.dockerignore` in each service (excluding `**/bin/`, `**/obj/`) or Windows paths leak into Linux container
- Can't `dotnet build` while `dotnet run` is active (DLL file locks)
- dotnet run uses Azure Cosmos DB, Docker Compose uses local MongoDB
