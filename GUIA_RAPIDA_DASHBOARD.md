# Guía Rápida: Crear Bounded Context de Dashboard

## DIAGRAMA DE LA ARQUITECTURA

```
┌─────────────────────────────────────────────────────────────────┐
│                      PRESENTACIÓN (UI)                          │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ↓
┌─────────────────────────────────────────────────────────────────┐
│               INTERFACES (Interfaces/REST)                      │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ DashboardController                                      │  │
│  │  - POST   /api/v1/dashboards          CreateDashboard  │  │
│  │  - GET    /api/v1/dashboards/{id}     GetDashboardById │  │
│  │  - GET    /api/v1/dashboards?userId   GetByUserId      │  │
│  │  - PUT    /api/v1/dashboards/{id}     UpdateDashboard  │  │
│  └──────────────────────────────────────────────────────────┘  │
│                         │                                       │
│  ┌──────────────────────┴───────────────────────────────────┐  │
│  │ Resources (DTOs) + Transform (Mappers)                  │  │
│  │ - CreateDashboardResource                               │  │
│  │ - DashboardResource                                     │  │
│  │ - *Assembler classes                                    │  │
│  └──────────────────────────────────────────────────────────┘  │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           ↓
┌──────────────────────────────────────────────────────────────────┐
│              APPLICATION (Application/Internal)                  │
│  ┌─────────────────────────┬──────────────────────────────────┐ │
│  │ CommandServices         │ QueryServices                    │ │
│  │                         │                                  │ │
│  │ - Create                │ - GetById                        │ │
│  │ - Update                │ - GetByUserId                    │ │
│  │ - Delete                │ - GetAll                         │ │
│  └──────┬──────────────────┴──────────────────┬───────────────┘ │
│         │                                      │                 │
│         └──────────────────┬───────────────────┘                 │
│                            │                                     │
│                            ↓                                     │
│                  ICommand/IQueryServices                         │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           ↓
┌──────────────────────────────────────────────────────────────────┐
│            DOMAIN (Domain - Lógica de Negocio)                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Aggregates (Raíces Agregadas)                            │  │
│  │ - Dashboard (Entidad Principal)                          │  │
│  │ - DashboardAudit (Auditoría)                             │  │
│  └──────────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Commands (Intención de Cambio)                           │  │
│  │ - CreateDashboardCommand                                 │  │
│  │ - UpdateDashboardCommand                                 │  │
│  └──────────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Queries (Intención de Lectura)                           │  │
│  │ - GetDashboardByIdQuery                                  │  │
│  │ - GetDashboardsByUserIdQuery                             │  │
│  └──────────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ ValueObjects (Valores Inmutables)                        │  │
│  │ - EDashboardType (enum)                                  │  │
│  │ - DashboardSettings                                      │  │
│  └──────────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Services (Interfaces)                                    │  │
│  │ - IDashboardCommandService                               │  │
│  │ - IDashboardQueryService                                 │  │
│  └──────────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Repositories (Interfaces de Persistencia)                │  │
│  │ - IDashboardRepository : IBaseRepository<Dashboard>      │  │
│  └──────────────────────────────────────────────────────────┘  │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           ↓
┌──────────────────────────────────────────────────────────────────┐
│         INFRASTRUCTURE (Infrastructure - Implementación)         │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Persistence/EFC/Repositories                             │  │
│  │ - DashboardRepository (Implementación)                   │  │
│  │   - FindByUserIdAsync()                                  │  │
│  │   - FindByNameAndUserIdAsync()                           │  │
│  └──────────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Shared/Infrastructure/Persistence/EFC                    │  │
│  │ - BaseRepository<T> (Implementación genérica)            │  │
│  │ - UnitOfWork (Patrón Transaccional)                      │  │
│  │ - AppDbContext (Entity Framework Core)                   │  │
│  └──────────────────────────────────────────────────────────┘  │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           ↓
┌──────────────────────────────────────────────────────────────────┐
│              BASE DE DATOS (MySQL)                              │
│  - dashboards table                                              │
│  - created_at, updated_at columns                                │
└──────────────────────────────────────────────────────────────────┘
```

---

## FLUJO DE UNA SOLICITUD WRITE (POST)

```
1. HTTP Request
   └─> POST /api/v1/dashboards
       Body: { "userId": 1, "name": "Sales", "type": "Analytics", "description": "..." }

2. Controller
   └─> DashboardController.CreateDashboard()
       └─> Recibe CreateDashboardResource
           └─> Valida ModelState
               └─> Llama Assembler

3. Mapper/Assembler
   └─> CreateDashboardCommandFromResourceAssembler.ToCommandFromResource()
       └─> Crea CreateDashboardCommand(userId, name, type, description)

4. Application Layer (Command Handler)
   └─> DashboardCommandService.Handle(CreateDashboardCommand)
       ├─> 1. Valida regla de negocio
       │      └─> ¿Existe dashboard con mismo nombre para este usuario?
       │
       ├─> 2. Crea agregado
       │      └─> new Dashboard(command)
       │
       ├─> 3. Persiste
       │      └─> dashboardRepository.AddAsync(dashboard)
       │      └─> unitOfWork.CompleteAsync()
       │
       └─> 4. Retorna agregado
               └─> return dashboard

5. Mapper/Assembler (Respuesta)
   └─> DashboardResourceFromEntityAssembler.ToResourceFromEntity()
       └─> Crea DashboardResource desde Dashboard

6. HTTP Response
   └─> 201 Created
       Location: /api/v1/dashboards/42
       Body: { "id": 42, "userId": 1, "name": "Sales", ... }
```

---

## FLUJO DE UNA SOLICITUD READ (GET)

```
1. HTTP Request
   └─> GET /api/v1/dashboards/42

2. Controller
   └─> DashboardController.GetDashboardById(42)
       └─> Crea GetDashboardByIdQuery(42)

3. Application Layer (Query Handler)
   └─> DashboardQueryService.Handle(GetDashboardByIdQuery)
       └─> Delega a repositorio
           └─> dashboardRepository.FindByIdAsync(42)

4. Infrastructure Layer
   └─> DashboardRepository.FindByIdAsync()
       └─> Context.Set<Dashboard>().FindAsync(42)
           └─> Busca en BD

5. Mapper/Assembler (Respuesta)
   └─> DashboardResourceFromEntityAssembler.ToResourceFromEntity()

6. HTTP Response
   └─> 200 OK
       Body: { "id": 42, "userId": 1, "name": "Sales", ... }
```

---

## LISTA DE ARCHIVOS A CREAR

```
Dashboard/
│
├── Domain/
│   ├── Model/
│   │   ├── Aggregates/
│   │   │   ├── Dashboard.cs                         [CREAR]
│   │   │   └── DashboardAudit.cs                    [CREAR]
│   │   ├── Commands/
│   │   │   ├── CreateDashboardCommand.cs            [CREAR]
│   │   │   └── UpdateDashboardCommand.cs            [CREAR]
│   │   ├── Queries/
│   │   │   ├── GetDashboardByIdQuery.cs             [CREAR]
│   │   │   ├── GetDashboardsByUserIdQuery.cs        [CREAR]
│   │   │   └── GetAllDashboardsQuery.cs             [CREAR]
│   │   └── ValueObjects/
│   │       └── EDashboardType.cs                    [CREAR]
│   ├── Repositories/
│   │   └── IDashboardRepository.cs                  [CREAR]
│   └── Services/
│       ├── IDashboardCommandService.cs              [CREAR]
│       └── IDashboardQueryService.cs                [CREAR]
│
├── Application/
│   └── Internal/
│       ├── CommandServices/
│       │   ├── DashboardCommandService.cs           [CREAR]
│       │   └── UpdateDashboardCommandService.cs     [CREAR]
│       └── QueryServices/
│           └── DashboardQueryService.cs             [CREAR]
│
├── Infrastructure/
│   └── Persistence/
│       └── EFC/
│           └── Repositories/
│               └── DashboardRepository.cs           [CREAR]
│
└── Interfaces/
    └── REST/
        ├── DashboardController.cs                   [CREAR]
        ├── Resources/
        │   ├── CreateDashboardResource.cs           [CREAR]
        │   ├── UpdateDashboardResource.cs           [CREAR]
        │   └── DashboardResource.cs                 [CREAR]
        └── Transform/
            ├── CreateDashboardCommandFromResourceAssembler.cs [CREAR]
            ├── UpdateDashboardCommandFromResourceAssembler.cs [CREAR]
            └── DashboardResourceFromEntityAssembler.cs        [CREAR]

EDICIONES REQUERIDAS:
├── Program.cs                                        [MODIFICAR - Agregar DI]
├── AppDbContext.cs                                   [MODIFICAR - Agregar Entity Config]
└── icetrack-platform.sln                             [VERIFICAR - Debe incluir carpeta]
```

---

## CHECKLIST DE IMPLEMENTACIÓN

### Fase 1: Domain Layer
- [ ] Crear carpeta Dashboard/Domain/Model/Aggregates/
- [ ] Crear Dashboard.cs con propiedades y constructores
- [ ] Crear DashboardAudit.cs (partial class)
- [ ] Crear carpeta Dashboard/Domain/Model/Commands/
- [ ] Crear CreateDashboardCommand.cs (record)
- [ ] Crear UpdateDashboardCommand.cs (record)
- [ ] Crear carpeta Dashboard/Domain/Model/Queries/
- [ ] Crear GetDashboardByIdQuery.cs (record)
- [ ] Crear GetDashboardsByUserIdQuery.cs (record)
- [ ] Crear GetAllDashboardsQuery.cs (record)
- [ ] Crear carpeta Dashboard/Domain/Model/ValueObjects/
- [ ] Crear EDashboardType.cs (enum)
- [ ] Crear carpeta Dashboard/Domain/Repositories/
- [ ] Crear IDashboardRepository.cs (interfaz)
- [ ] Crear carpeta Dashboard/Domain/Services/
- [ ] Crear IDashboardCommandService.cs (interfaz)
- [ ] Crear IDashboardQueryService.cs (interfaz)

### Fase 2: Application Layer
- [ ] Crear carpeta Dashboard/Application/Internal/CommandServices/
- [ ] Crear DashboardCommandService.cs (implementación)
- [ ] Crear UpdateDashboardCommandService.cs (implementación)
- [ ] Crear carpeta Dashboard/Application/Internal/QueryServices/
- [ ] Crear DashboardQueryService.cs (implementación)

### Fase 3: Infrastructure Layer
- [ ] Crear carpeta Dashboard/Infrastructure/Persistence/EFC/Repositories/
- [ ] Crear DashboardRepository.cs (implementación)

### Fase 4: Interfaces Layer
- [ ] Crear carpeta Dashboard/Interfaces/REST/
- [ ] Crear DashboardController.cs
- [ ] Crear carpeta Dashboard/Interfaces/REST/Resources/
- [ ] Crear CreateDashboardResource.cs (record)
- [ ] Crear UpdateDashboardResource.cs (record)
- [ ] Crear DashboardResource.cs (record)
- [ ] Crear carpeta Dashboard/Interfaces/REST/Transform/
- [ ] Crear CreateDashboardCommandFromResourceAssembler.cs
- [ ] Crear UpdateDashboardCommandFromResourceAssembler.cs
- [ ] Crear DashboardResourceFromEntityAssembler.cs

### Fase 5: Configuración
- [ ] Actualizar Program.cs con registros DI
- [ ] Actualizar AppDbContext.cs con entidad Dashboard
- [ ] Ejecutar migraciones EF Core
- [ ] Probar endpoints en Swagger

---

## TEMPLATES DE CÓDIGO

### Template 1: Agregado (Aggregate Root)
```csharp
namespace IceTrackPlatform.API.Dashboard.Domain.Model.Aggregates;

public partial class Dashboard
{
    public int Id { get; }
    public int UserId { get; private set; }
    public string Name { get; private set; }
    public EDashboardType Type { get; private set; }
    public string Description { get; private set; }
    public bool IsActive { get; private set; }
    
    // Constructor principal
    public Dashboard(int userId, string name, EDashboardType type, 
                     string description, bool isActive = true)
    {
        UserId = userId;
        Name = name;
        Type = type;
        Description = description;
        IsActive = isActive;
    }
    
    // Constructor desde Command
    public Dashboard(CreateDashboardCommand command)
    {
        UserId = command.UserId;
        Name = command.Name;
        Type = command.Type;
        Description = command.Description;
        IsActive = true;
    }
    
    // Métodos de negocio
    public void Update(string name, string description, bool isActive)
    {
        Name = name;
        Description = description;
        IsActive = isActive;
    }
    
    public void Deactivate()
    {
        IsActive = false;
    }
}
```

### Template 2: Command
```csharp
namespace IceTrackPlatform.API.Dashboard.Domain.Model.Commands;

public record CreateDashboardCommand(
    int UserId,
    string Name,
    EDashboardType Type,
    string Description);

public record UpdateDashboardCommand(
    int Id,
    string Name,
    string Description,
    bool IsActive);
```

### Template 3: Query
```csharp
namespace IceTrackPlatform.API.Dashboard.Domain.Model.Queries;

public record GetDashboardByIdQuery(int Id);
public record GetDashboardsByUserIdQuery(int UserId);
public record GetAllDashboardsQuery();
```

### Template 4: Repositorio Interface
```csharp
namespace IceTrackPlatform.API.Dashboard.Domain.Repositories;

public interface IDashboardRepository : IBaseRepository<Dashboard>
{
    Task<IEnumerable<Dashboard>> FindByUserIdAsync(int userId);
    Task<Dashboard?> FindByNameAndUserIdAsync(string name, int userId);
}
```

### Template 5: Repositorio Implementación
```csharp
namespace IceTrackPlatform.API.Dashboard.Infrastructure.Persistence.EFC.Repositories;

public class DashboardRepository(AppDbContext context)
    : BaseRepository<Dashboard>(context), IDashboardRepository
{
    public async Task<IEnumerable<Dashboard>> FindByUserIdAsync(int userId)
    {
        return await Context.Set<Dashboard>()
            .Where(d => d.UserId == userId)
            .ToListAsync();
    }
    
    public async Task<Dashboard?> FindByNameAndUserIdAsync(string name, int userId)
    {
        return await Context.Set<Dashboard>()
            .FirstOrDefaultAsync(d => d.UserId == userId && d.Name == name);
    }
}
```

### Template 6: Command Service
```csharp
namespace IceTrackPlatform.API.Dashboard.Application.Internal.CommandServices;

public class DashboardCommandService(
    IDashboardRepository dashboardRepository,
    IUnitOfWork unitOfWork) : IDashboardCommandService
{
    public async Task<Dashboard?> Handle(CreateDashboardCommand command)
    {
        var existing = await dashboardRepository
            .FindByNameAndUserIdAsync(command.Name, command.UserId);
        
        if (existing != null)
            throw new Exception("Dashboard with this name already exists");

        var dashboard = new Dashboard(command);
        await dashboardRepository.AddAsync(dashboard);
        await unitOfWork.CompleteAsync();

        return dashboard;
    }
    
    public async Task<Dashboard?> Handle(UpdateDashboardCommand command)
    {
        var dashboard = await dashboardRepository.FindByIdAsync(command.Id);
        if (dashboard == null)
            return null;

        dashboard.Update(command.Name, command.Description, command.IsActive);
        dashboardRepository.Update(dashboard);
        await unitOfWork.CompleteAsync();

        return dashboard;
    }
}
```

### Template 7: Query Service
```csharp
namespace IceTrackPlatform.API.Dashboard.Application.Internal.QueryServices;

public class DashboardQueryService(IDashboardRepository dashboardRepository)
    : IDashboardQueryService
{
    public async Task<Dashboard?> Handle(GetDashboardByIdQuery query)
    {
        return await dashboardRepository.FindByIdAsync(query.Id);
    }
    
    public async Task<IEnumerable<Dashboard>> Handle(GetDashboardsByUserIdQuery query)
    {
        return await dashboardRepository.FindByUserIdAsync(query.UserId);
    }
    
    public async Task<IEnumerable<Dashboard>> Handle(GetAllDashboardsQuery query)
    {
        return await dashboardRepository.ListAsync();
    }
}
```

### Template 8: Controller
```csharp
namespace IceTrackPlatform.API.Dashboard.Interfaces.REST;

[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Dashboard")]
public class DashboardController(
    IDashboardCommandService commandService,
    IDashboardQueryService queryService) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(Summary = "Create a new Dashboard", OperationId = "CreateDashboard")]
    [SwaggerResponse(201, "Created dashboard", typeof(DashboardResource))]
    [SwaggerResponse(400, "Invalid input")]
    [SwaggerResponse(409, "Dashboard already exists")]
    public async Task<IActionResult> CreateDashboard([FromBody] CreateDashboardResource resource)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

        var command = CreateDashboardCommandFromResourceAssembler.ToCommandFromResource(resource);
        
        try
        {
            var result = await commandService.Handle(command);
            if (result is null) return BadRequest();
            
            return CreatedAtAction(nameof(GetDashboardById), new { id = result.Id },
                DashboardResourceFromEntityAssembler.ToResourceFromEntity(result));
        }
        catch (Exception e) when (e.Message.Contains("already exists"))
        {
            return Conflict(e.Message);
        }
        catch
        {
            return BadRequest();
        }
    }
    
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get Dashboard by ID", OperationId = "GetDashboardById")]
    [SwaggerResponse(200, "Dashboard found", typeof(DashboardResource))]
    [SwaggerResponse(404, "Dashboard not found")]
    public async Task<IActionResult> GetDashboardById(int id)
    {
        var query = new GetDashboardByIdQuery(id);
        var result = await queryService.Handle(query);
        if (result is null) return NotFound();
        
        return Ok(DashboardResourceFromEntityAssembler.ToResourceFromEntity(result));
    }
}
```

---

## CONFIGURACIÓN EN Program.cs

```csharp
// Agregaar después de las inyecciones del Reporting Context

// Dashboard Context Injections
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IDashboardQueryService, DashboardQueryService>();
builder.Services.AddScoped<IDashboardCommandService, DashboardCommandService>();
```

---

## CONFIGURACIÓN EN AppDbContext.cs

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);
    
    // Reporting Context
    builder.Entity<Report>().HasKey(r => r.Id);
    builder.Entity<Report>().Property(r => r.Id).IsRequired().ValueGeneratedOnAdd();
    builder.Entity<Report>().Property(r => r.TenantId).IsRequired();
    builder.Entity<Report>().Property(r => r.EquipmentId).IsRequired();
    builder.Entity<Report>().Property(r => r.Status).HasConversion<string>().IsRequired();
    
    // Dashboard Context
    builder.Entity<Dashboard>().HasKey(d => d.Id);
    builder.Entity<Dashboard>().Property(d => d.Id).IsRequired().ValueGeneratedOnAdd();
    builder.Entity<Dashboard>().Property(d => d.UserId).IsRequired();
    builder.Entity<Dashboard>().Property(d => d.Name).IsRequired();
    builder.Entity<Dashboard>().Property(d => d.Type).HasConversion<string>().IsRequired();
    builder.Entity<Dashboard>().Property(d => d.IsActive).IsRequired();
    
    // General Naming Convention
    builder.UseSnakeCaseNamingConvention();
}
```

---

## COMANDOS ÚTILES

```bash
# Compilar solución
dotnet build

# Ejecutar tests
dotnet test

# Crear migración
dotnet ef migrations add AddDashboardContext -p IceTrackPlatform.API

# Aplicar migración
dotnet ef database update

# Limpiar y reconstruir
dotnet clean && dotnet build

# Ver rutas disponibles
# En Swagger: http://localhost:5000/swagger/ui
```

---

## TESTING MANUAL (Postman/Insomnia)

### Crear Dashboard
```http
POST /api/v1/dashboards HTTP/1.1
Host: localhost:5000
Content-Type: application/json

{
  "userId": 1,
  "name": "Sales Dashboard",
  "type": "Analytics",
  "description": "Track sales metrics"
}
```

### Obtener Dashboard
```http
GET /api/v1/dashboards/1 HTTP/1.1
Host: localhost:5000
```

### Obtener Dashboards por Usuario
```http
GET /api/v1/dashboards?userId=1 HTTP/1.1
Host: localhost:5000
```

### Actualizar Dashboard
```http
PUT /api/v1/dashboards/1 HTTP/1.1
Host: localhost:5000
Content-Type: application/json

{
  "name": "Sales Dashboard Updated",
  "description": "Updated description",
  "isActive": true
}
```

---

## PUNTOS CLAVE A RECORDAR

1. **Nomenclatura**: Mantener consistencia con Reporting context
2. **Immutabilidad**: Commands y Queries como records
3. **Separación**: Domain/Application/Infrastructure son independientes
4. **DI**: Siempre usar interfaces, nunca clases concretas
5. **Async/Await**: Todo debe ser asincrónico
6. **Validación**: En servicios de aplicación, no en controllers
7. **Transaccionalidad**: Siempre usar UnitOfWork.CompleteAsync()
8. **Logging**: Considerar agregar logs en servicios
9. **Documentación**: Mantener comentarios XML en métodos públicos
10. **Testing**: Preparar proyecto para unit tests

---

## REFERENCIAS

- Documento completo: `/tmp/ARQUITECTURA_ICETRACK.md`
- Bounded context existente: `Reporting/`
- Código compartido: `Shared/`
- Entry point: `Program.cs`
- Context: `AppDbContext.cs`
