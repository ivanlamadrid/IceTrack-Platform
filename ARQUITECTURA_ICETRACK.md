# Análisis Arquitectónico: IceTrack-Platform

## 1. ESTRUCTURA GENERAL DEL PROYECTO

### Bounded Contexts Existentes:
```
IceTrackPlatform.API/
├── Reporting/        ← Bounded Context 1
├── Shared/          ← Código compartido entre contextos
├── Program.cs       ← Entry point y configuración DI
└── Properties/
```

### Nota Importante:
- Se menciona en código comentarios de "IAM Context" pero aún no está implementado
- El patrón está preparado para futuras expansiones

---

## 2. ARQUITECTURA POR CAPAS (Clean Architecture + DDD)

Cada Bounded Context implementa las siguientes capas:

### 2.1 Estructura de Carpetas del Bounded Context "Reporting"

```
Reporting/
├── Domain/
│   ├── Model/
│   │   ├── Aggregates/          ← Raíces agregadas
│   │   │   ├── Report.cs
│   │   │   └── ReportAudit.cs
│   │   ├── Commands/            ← Comandos CQRS
│   │   │   └── CreateReportCommand.cs
│   │   ├── Queries/             ← Queries CQRS
│   │   │   ├── GetReportByIdQuery.cs
│   │   │   ├── GetAllReportsByTenantIdQuery.cs
│   │   │   └── GetAllReportsByEquipmentIdQuery.cs
│   │   └── ValueObjects/        ← Value Objects
│   │       └── EReportType.cs
│   ├── Repositories/            ← Abstracciones de persistencia
│   │   └── IReportRepository.cs
│   └── Services/                ← Servicios de Dominio
│       ├── IReportCommandService.cs
│       └── IReportQueryServices.cs
│
├── Application/
│   └── Internal/
│       ├── CommandServices/     ← Implementación de Handlers
│       │   └── ReportCommandService.cs
│       └── QueryServices/       ← Implementación de Queries
│           └── ReportQueryService.cs
│
├── Infrastructure/
│   └── Persistence/
│       └── EFC/                 ← Entity Framework Core
│           └── Repositories/
│               └── ReportRepository.cs
│
└── Interfaces/
    └── REST/
        ├── ReportController.cs  ← Controlador REST
        ├── Resources/           ← DTOs
        │   ├── CreateReportResource.cs
        │   └── ReportResource.cs
        └── Transform/           ← Ensambladores (Mappers)
            ├── CreateReportCommandFromResourceAssembler.cs
            └── ReportResourceFromEntityAssembler.cs
```

---

## 3. IMPLEMENTACIÓN DDD (Domain-Driven Design)

### 3.1 Agregados (Aggregates)

**Ubicación:** `Domain/Model/Aggregates/`

```csharp
// Report.cs - AGREGADO RAÍZ
public partial class Report
{
    public int Id { get; }                          // Entity ID
    public int TenantId { get; private set; }       // Tenant Reference
    public string Type { get; private set; }
    public int EquipmentId { get; private set; }
    public string Title { get; private set; }
    public EReportType Status { get; private set; } // Value Object
    public string Summary { get; private set; }
    public string Content { get; private set; }
    public string Url { get; private set; }
    
    // Constructor principal
    public Report(int tenantId, string type, int equipmentId, ...)
    {
        // Inicialización de propiedades
    }
    
    // Constructor desde Command (patrón)
    public Report(CreateReportCommand command)
    {
        TenantId = command.TenantId;
        Type = command.Type;
        // ...
    }
}

// ReportAudit.cs - ENTIDAD DE AUDITORÍA
public partial class ReportAudit : IEntityWithCreatedUpdatedDate
{
    public DateTimeOffset? CreatedDate { get; set; }
    public DateTimeOffset? UpdatedDate { get; set; }
}
```

**Características DDD:**
- Identidad única (Id)
- Encapsulación de lógica
- Propiedades privadas con getter público
- Value Objects integrados (EReportType)
- Constructor desde Commands
- Partial class para separar lógica de auditoría

---

### 3.2 Value Objects

**Ubicación:** `Domain/Model/ValueObjects/`

```csharp
// EReportType.cs - VALUE OBJECT (Enum)
public enum EReportType
{
    InProgress,
    Completed,
    Failed
}
```

**Características:**
- Enums para valores limitados
- Se convierte a string en BD (string converter)
- Immutable
- Sin identidad propia

---

### 3.3 Repositorios (Repository Pattern)

**Ubicación:** `Domain/Repositories/` (Interfaz) y `Infrastructure/Persistence/EFC/Repositories/` (Implementación)

```csharp
// IReportRepository.cs - INTERFAZ
public interface IReportRepository : IBaseRepository<Report>
{
    Task<IEnumerable<Report>> FindByTenantIdAsync(int tenantId);
    Task<IEnumerable<Report>> FindByEquipmentIdAsync(int equipmentId);
}

// ReportRepository.cs - IMPLEMENTACIÓN
public class ReportRepository(AppDbContext context)
    : BaseRepository<Report>(context), IReportRepository
{
    public async Task<IEnumerable<Report>> FindByTenantIdAsync(int tenantId)
    {
        return await Context.Set<Report>()
            .Where(r => r.TenantId == tenantId)
            .ToListAsync();
    }
}
```

**Patrón implementado:**
- Abstracción en Domain, implementación en Infrastructure
- Herencia de BaseRepository<T>
- Métodos específicos del contexto
- Acceso a DbContext protegido

---

### 3.4 Servicios de Dominio

**Ubicación:** `Domain/Services/` (Interfaces)

```csharp
// IReportCommandService.cs
public interface IReportCommandService
{
    Task<Report?> Handle(CreateReportCommand command);
}

// IReportQueryServices.cs
public interface IReportQueryServices
{
    Task<IEnumerable<Report>> Handle(GetAllReportsByTenantIdQuery query);
    Task<IEnumerable<Report>> Handle(GetAllReportsByEquipmentIdQuery query);
    Task<Report?> Handle(GetReportByIdQuery query);
}
```

**Responsabilidades:**
- Definir contrato para operaciones complejas
- Separar lectura (Query) de escritura (Command)
- Orquestar la lógica de negocio

---

## 4. IMPLEMENTACIÓN CQRS (Command Query Responsibility Segregation)

### 4.1 Commands (Escritura)

**Ubicación:** `Domain/Model/Commands/`

```csharp
// CreateReportCommand.cs
public record CreateReportCommand(
    int TenantId, 
    string Type, 
    int EquipmentId, 
    string Title,
    string Summary, 
    string Content, 
    string Url);
```

**Características CQRS:**
- Declaradas como records (immutable)
- Representa una intención de cambio
- Nombres imperativo (Create, Update, Delete)
- Mapeo desde Resources (DTOs)

---

### 4.2 Queries (Lectura)

**Ubicación:** `Domain/Model/Queries/`

```csharp
// GetReportByIdQuery.cs
public record GetReportByIdQuery(int Id);

// GetAllReportsByTenantIdQuery.cs
public record GetAllReportsByTenantIdQuery(int TenantId);

// GetAllReportsByEquipmentIdQuery.cs
public record GetAllReportsByEquipmentIdQuery(int EquipmentId);
```

**Características CQRS:**
- Records inmutables
- Nombres descriptivos (Get)
- Parámetros específicos
- No modifican estado

---

### 4.3 Command Handlers

**Ubicación:** `Application/Internal/CommandServices/`

```csharp
// ReportCommandService.cs
public class ReportCommandService(
    IReportRepository reportRepository,
    IUnitOfWork unitOfWork) : IReportCommandService
{
    public async Task<Report?> Handle(CreateReportCommand command)
    {
        // 1. VALIDACIÓN DE REGLAS DE NEGOCIO
        var existingReports = await reportRepository
            .FindByEquipmentIdAsync(command.EquipmentId);
        
        if (existingReports.Any(r => 
            r.Type == command.Type && r.Title == command.Title))
            throw new Exception("Report already exists");

        // 2. CREAR AGREGADO desde Command
        var report = new Report(command);

        // 3. PERSISTIR
        await reportRepository.AddAsync(report);
        await unitOfWork.CompleteAsync();

        // 4. RETORNAR AGREGADO
        return report;
    }
}
```

**Patrón:**
1. Validar reglas de negocio
2. Crear/Modificar agregados
3. Guardar vía Repository + UnitOfWork
4. Retornar resultado

---

### 4.4 Query Handlers

**Ubicación:** `Application/Internal/QueryServices/`

```csharp
// ReportQueryService.cs
public class ReportQueryService(IReportRepository reportRepository)
    : IReportQueryServices
{
    public async Task<IEnumerable<Report>> Handle(GetAllReportsByTenantIdQuery query)
    {
        return await reportRepository.FindByTenantIdAsync(query.TenantId);
    }
    
    public async Task<Report?> Handle(GetReportByIdQuery query)
    {
        return await reportRepository.FindByIdAsync(query.Id);
    }
}
```

**Características:**
- Solo lectura
- Delegan al repositorio
- No realizan mutaciones
- Pueden ser fácilmente cachés

---

## 5. PATRONES DE ARQUITECTURA IMPLEMENTADOS

### 5.1 Repository Pattern
- Abstracción de la persistencia
- IBaseRepository<T> para operaciones comunes
- Repositorios específicos para operaciones personalizadas
- Implementación con Entity Framework Core

### 5.2 Unit of Work Pattern
```csharp
// IUnitOfWork.cs
public interface IUnitOfWork
{
    Task CompleteAsync(); // Guarda TODOS los cambios
}

// UnitOfWork.cs
public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task CompleteAsync()
    {
        await context.SaveChangesAsync(); // Transacción
    }
}
```

### 5.3 Mapper/Assembler Pattern
```csharp
// CreateReportCommandFromResourceAssembler.cs
public static class CreateReportCommandFromResourceAssembler
{
    public static CreateReportCommand ToCommandFromResource(CreateReportResource resource) =>
        new CreateReportCommand(
            resource.TenantId, resource.Type, resource.EquipmentId,
            resource.Title, resource.Summary, resource.Content, resource.Url);
}

// ReportResourceFromEntityAssembler.cs
public static class ReportResourceFromEntityAssembler
{
    public static ReportResource ToResourceFromEntity(Report entity) =>
        new ReportResource(
            entity.Id, entity.TenantId, entity.Type, entity.EquipmentId,
            entity.Title, entity.Status.ToString(), 
            entity.Summary, entity.Content, entity.Url);
}
```

### 5.4 DTO Pattern
```csharp
// CreateReportResource.cs - INPUT DTO
public record CreateReportResource(
    [Required] int TenantId,
    [Required] string Type,
    [Required] int EquipmentId,
    [Required] string Title,
    [Required] string Summary,
    [Required] string Content,
    [Required] string Url);

// ReportResource.cs - OUTPUT DTO
public record ReportResource(
    int Id, int TenantId, string Type, int EquipmentId,
    string Title, string Status, string Summary, string Content, string Url);
```

---

## 6. FLUJO DE SOLICITUD (Request Flow)

### 6.1 Escribir (Command)
```
HTTP POST /api/v1/reports
    ↓
ReportController.CreateReport(CreateReportResource)
    ↓
CreateReportCommandFromResourceAssembler.ToCommandFromResource()
    ↓
ReportCommandService.Handle(CreateReportCommand)
    ├─ Validar reglas de negocio
    ├─ Crear Report(command)
    ├─ reportRepository.AddAsync(report)
    └─ unitOfWork.CompleteAsync()
    ↓
ReportResourceFromEntityAssembler.ToResourceFromEntity()
    ↓
HTTP 201 Created
```

### 6.2 Leer (Query)
```
HTTP GET /api/v1/reports/{id}
    ↓
ReportController.GetReportById(id)
    ↓
GetReportByIdQuery(id)
    ↓
ReportQueryService.Handle(query)
    ↓
reportRepository.FindByIdAsync(id)
    ↓
ReportResourceFromEntityAssembler.ToResourceFromEntity()
    ↓
HTTP 200 OK
```

---

## 7. CONFIGURACIÓN DE INYECCIÓN DE DEPENDENCIAS

**Ubicación:** `Program.cs`

```csharp
// Database Configuration
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySQL(connectionString)
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging();
});

// Shared Context
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Reporting Context
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IReportQueryServices, ReportQueryService>();
builder.Services.AddScoped<IReportCommandService, ReportCommandService>();
```

**Patrón:**
- Ciclo de vida: Scoped (una instancia por request HTTP)
- Registrar interfaces en DI
- Inyectar dependencias en constructores

---

## 8. ENTIDAD COMPARTIDA: AppDbContext

```csharp
public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Configuración por contexto
        builder.Entity<Report>().HasKey(r => r.Id);
        builder.Entity<Report>().Property(r => r.Status)
            .HasConversion<string>()
            .IsRequired();
        
        // Convención global
        builder.UseSnakeCaseNamingConvention();
    }
}
```

**Características:**
- Única BD para múltiples contextos
- Configuración centralizada
- Mapeo automático a snake_case
- Soporte para auditoría (CreatedDate, UpdatedDate)

---

## 9. PAQUETES UTILIZADOS

```xml
<!-- ORM -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.10"/>
<PackageReference Include="MySql.EntityFrameworkCore" Version="9.0.9"/>

<!-- Auditoría -->
<PackageReference Include="EntityFrameworkCore.CreatedUpdatedDate" Version="8.0.0"/>

<!-- Mediator/CQRS (opcional, disponible para futuro) -->
<PackageReference Include="Cortex.Mediator" Version="2.1.0"/>

<!-- API/Documentación -->
<PackageReference Include="Swashbuckle.AspNetCore" Version="9.0.6"/>
<PackageReference Include="Swashbuckle.AspNetCore.Annotations" Version="9.0.6"/>

<!-- Utilidades -->
<PackageReference Include="Humanizer" Version="2.14.1"/>
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3"/>
```

---

## 10. DOCUMENTACIÓN API

**Característica:** Swagger/OpenAPI automático

```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Report")]
public class ReportController
{
    [HttpPost]
    [SwaggerOperation(Summary = "Create a new Report", OperationId = "CreateReport")]
    [SwaggerResponse(201, "Created report", typeof(ReportResource))]
    [SwaggerResponse(400, "The Report was not created")]
    public async Task<IActionResult> CreateReport([FromBody] CreateReportResource resource)
    { ... }
}
```

---

## 11. PLANTILLA: CÓMO CREAR UN NUEVO BOUNDED CONTEXT (Dashboard)

### 11.1 Estructura de Carpetas
```
Dashboard/
├── Domain/
│   ├── Model/
│   │   ├── Aggregates/
│   │   │   ├── Dashboard.cs
│   │   │   └── DashboardAudit.cs
│   │   ├── Commands/
│   │   │   ├── CreateDashboardCommand.cs
│   │   │   └── UpdateDashboardCommand.cs
│   │   ├── Queries/
│   │   │   ├── GetDashboardByIdQuery.cs
│   │   │   ├── GetDashboardsByUserIdQuery.cs
│   │   │   └── GetAllDashboardsQuery.cs
│   │   └── ValueObjects/
│   │       ├── EDashboardType.cs
│   │       └── DashboardSettings.cs
│   ├── Repositories/
│   │   └── IDashboardRepository.cs
│   └── Services/
│       ├── IDashboardCommandService.cs
│       └── IDashboardQueryService.cs
│
├── Application/
│   └── Internal/
│       ├── CommandServices/
│       │   ├── DashboardCommandService.cs
│       │   └── UpdateDashboardCommandService.cs
│       └── QueryServices/
│           └── DashboardQueryService.cs
│
├── Infrastructure/
│   └── Persistence/
│       └── EFC/
│           └── Repositories/
│               └── DashboardRepository.cs
│
└── Interfaces/
    └── REST/
        ├── DashboardController.cs
        ├── Resources/
        │   ├── CreateDashboardResource.cs
        │   ├── UpdateDashboardResource.cs
        │   └── DashboardResource.cs
        └── Transform/
            ├── CreateDashboardCommandFromResourceAssembler.cs
            ├── UpdateDashboardCommandFromResourceAssembler.cs
            └── DashboardResourceFromEntityAssembler.cs
```

### 11.2 Pasos de Implementación

#### Paso 1: Definir el Agregado
```csharp
// Dashboard.cs
namespace IceTrackPlatform.API.Dashboard.Domain.Model.Aggregates;

public partial class Dashboard
{
    public int Id { get; }
    public int UserId { get; private set; }
    public string Name { get; private set; }
    public EDashboardType Type { get; private set; }
    public string Description { get; private set; }
    public bool IsActive { get; private set; }
    
    public Dashboard(int userId, string name, EDashboardType type, 
                     string description, bool isActive = true)
    {
        UserId = userId;
        Name = name;
        Type = type;
        Description = description;
        IsActive = isActive;
    }
    
    public Dashboard(CreateDashboardCommand command)
    {
        UserId = command.UserId;
        Name = command.Name;
        Type = command.Type;
        Description = command.Description;
        IsActive = true;
    }
}
```

#### Paso 2: Definir Commands y Queries
```csharp
// Commands
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

// Queries
public record GetDashboardByIdQuery(int Id);
public record GetDashboardsByUserIdQuery(int UserId);
public record GetAllDashboardsQuery();
```

#### Paso 3: Definir Repositorio
```csharp
// IDashboardRepository.cs
public interface IDashboardRepository : IBaseRepository<Dashboard>
{
    Task<IEnumerable<Dashboard>> FindByUserIdAsync(int userId);
    Task<Dashboard?> FindByNameAndUserIdAsync(string name, int userId);
}

// DashboardRepository.cs
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

#### Paso 4: Definir Servicios
```csharp
// IDashboardCommandService.cs
public interface IDashboardCommandService
{
    Task<Dashboard?> Handle(CreateDashboardCommand command);
    Task<Dashboard?> Handle(UpdateDashboardCommand command);
}

// DashboardCommandService.cs
public class DashboardCommandService(
    IDashboardRepository dashboardRepository,
    IUnitOfWork unitOfWork) : IDashboardCommandService
{
    public async Task<Dashboard?> Handle(CreateDashboardCommand command)
    {
        var existing = await dashboardRepository
            .FindByNameAndUserIdAsync(command.Name, command.UserId);
        
        if (existing != null)
            throw new Exception("Dashboard with this name already exists for user");

        var dashboard = new Dashboard(command);
        await dashboardRepository.AddAsync(dashboard);
        await unitOfWork.CompleteAsync();

        return dashboard;
    }
}
```

#### Paso 5: Definir DTOs
```csharp
// CreateDashboardResource.cs
public record CreateDashboardResource(
    [Required] int UserId,
    [Required] string Name,
    [Required] string Type,
    [Required] string Description);

// DashboardResource.cs
public record DashboardResource(
    int Id,
    int UserId,
    string Name,
    string Type,
    string Description,
    bool IsActive);
```

#### Paso 6: Definir Mappers
```csharp
// CreateDashboardCommandFromResourceAssembler.cs
public static class CreateDashboardCommandFromResourceAssembler
{
    public static CreateDashboardCommand ToCommandFromResource(CreateDashboardResource resource) =>
        new CreateDashboardCommand(
            resource.UserId,
            resource.Name,
            Enum.Parse<EDashboardType>(resource.Type),
            resource.Description);
}

// DashboardResourceFromEntityAssembler.cs
public static class DashboardResourceFromEntityAssembler
{
    public static DashboardResource ToResourceFromEntity(Dashboard entity) =>
        new DashboardResource(
            entity.Id,
            entity.UserId,
            entity.Name,
            entity.Type.ToString(),
            entity.Description,
            entity.IsActive);
}
```

#### Paso 7: Definir Controlador
```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Dashboard")]
public class DashboardController(
    IDashboardCommandService commandService,
    IDashboardQueryService queryService) : ControllerBase
{
    [HttpPost]
    [SwaggerResponse(201, "Created dashboard", typeof(DashboardResource))]
    public async Task<IActionResult> CreateDashboard(
        [FromBody] CreateDashboardResource resource)
    {
        var command = CreateDashboardCommandFromResourceAssembler
            .ToCommandFromResource(resource);
        
        try
        {
            var result = await commandService.Handle(command);
            return CreatedAtAction(nameof(GetDashboardById), 
                new { id = result.Id },
                DashboardResourceFromEntityAssembler.ToResourceFromEntity(result));
        }
        catch (Exception e) when (e.Message.Contains("already exists"))
        {
            return Conflict(e.Message);
        }
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDashboardById(int id)
    {
        var query = new GetDashboardByIdQuery(id);
        var result = await queryService.Handle(query);
        if (result is null) return NotFound();
        
        return Ok(DashboardResourceFromEntityAssembler.ToResourceFromEntity(result));
    }
}
```

#### Paso 8: Registrar en Program.cs
```csharp
// Dashboard Context Injections
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IDashboardQueryService, DashboardQueryService>();
builder.Services.AddScoped<IDashboardCommandService, DashboardCommandService>();
```

#### Paso 9: Configurar en AppDbContext
```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);
    
    // Dashboard Configuration
    builder.Entity<Dashboard>().HasKey(d => d.Id);
    builder.Entity<Dashboard>().Property(d => d.Id)
        .IsRequired().ValueGeneratedOnAdd();
    builder.Entity<Dashboard>().Property(d => d.UserId).IsRequired();
    builder.Entity<Dashboard>().Property(d => d.Name).IsRequired();
    builder.Entity<Dashboard>().Property(d => d.Type)
        .HasConversion<string>().IsRequired();
    builder.Entity<Dashboard>().Property(d => d.IsActive).IsRequired();
    
    builder.UseSnakeCaseNamingConvention();
}
```

---

## 12. CONVENCIONES CLAVE

1. **Namespacing:**
   - `IceTrackPlatform.API.{BoundedContext}.Domain...`
   - `IceTrackPlatform.API.{BoundedContext}.Application...`
   - `IceTrackPlatform.API.{BoundedContext}.Infrastructure...`
   - `IceTrackPlatform.API.{BoundedContext}.Interfaces...`

2. **Naming:**
   - Commands: `{Verb}{EntityName}Command` (CreateReportCommand)
   - Queries: `{Get}{EntityName(s)}{Filter?}Query` (GetReportByIdQuery)
   - Services: `{Entity}{Operation}Service` (ReportCommandService)
   - Repositories: `{Entity}Repository` (ReportRepository)
   - Controllers: `{Entity}Controller` (ReportController)

3. **Records vs Classes:**
   - **Records:** Commands, Queries, Resources (DTOs) - Immutable
   - **Classes:** Aggregates, Entities, Services - Mutables

4. **Access Modifiers:**
   - Properties públicas solo lectura (public get)
   - Setters privados (private set) en Aggregates
   - Métodos públicos para modificar estado

5. **Rutas API:**
   - Patrón: `/api/v1/{controller}`
   - Ejemplo: `/api/v1/reports`, `/api/v1/dashboards`

6. **Respuestas HTTP:**
   - 201 Created: POST exitoso
   - 400 Bad Request: Validación fallida
   - 404 Not Found: Recurso no existe
   - 409 Conflict: Duplicado o conflicto de negocio

---

## 13. VENTAJAS DE ESTA ARQUITECTURA

1. **Separación de Responsabilidades:** Cada capa tiene un propósito claro
2. **Testabilidad:** Interfaces permiten mocking fácil
3. **Escalabilidad:** Bounded contexts independientes
4. **Mantenibilidad:** Código organizado y predecible
5. **Aislamiento:** Cambios en un contexto no afectan otros
6. **DDD puro:** Lógica de negocio en Domain, no en Controllers
7. **CQRS clean:** Separación de lectura y escritura explícita
8. **Auditoría integrada:** CreatedDate/UpdatedDate automático

---

## 14. PRÓXIMAS EXPANSIONES

- **Implementar IAM Context** (comentado en AppDbContext)
- **Eventos de Dominio:** Publicar eventos al crear/actualizar agregados
- **Mediator Cortex:** Usar library instalada para mediator pattern explícito
- **Validadores:** Fluent Validation o similar
- **Unit Testing:** xUnit con Moq
- **Integration Testing:** TestContainers para BD
- **Logging:** Serilog para structured logging

---

## Conclusión

La arquitectura implementada en IceTrack-Platform es un excelente ejemplo de cómo combinar:
- **Clean Architecture** (separación clara de capas)
- **Domain-Driven Design** (lógica en Domain)
- **CQRS** (lectura vs escritura)
- **Repository Pattern** (abstracción de persistencia)
- **Unit of Work** (transaccionalidad)
- **.NET Best Practices** (DI, async/await, records)

Para el nuevo bounded context de Dashboard, siga los patrones descritos en la sección 11 para mantener consistencia y aplicar las mismas prácticas arquitectónicas.
