using System.Text.Json.Serialization;
using IceTrackPlatform.API.Dashboard.Application.Internal.CommandServices;
using IceTrackPlatform.API.Dashboard.Application.Internal.QueryServices;
using IceTrackPlatform.API.Dashboard.Domain.Repositories;
using IceTrackPlatform.API.Dashboard.Domain.Services;
using IceTrackPlatform.API.Dashboard.Infrastructure.Persistence.EFC.Repositories;
using IceTrackPlatform.API.Reporting.Application.Internal.CommandServices;
using IceTrackPlatform.API.Reporting.Application.Internal.QueryServices;
using IceTrackPlatform.API.Reporting.Domain.Repositories;
using IceTrackPlatform.API.Reporting.Domain.Services;
using IceTrackPlatform.API.Reporting.Infrastructure.Persistence.EFC.Repositories;
using IceTrackPlatform.API.Shared.Domain.Repositories;
using IceTrackPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using IceTrackPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure Lower Case URLs
builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.EnableAnnotations());

// Add Database Connection

// Configuration Database Context and Logging Levels
if (builder.Environment.IsDevelopment())
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (connectionString is null)
            throw new Exception("Connection string is null");
        options.UseMySQL(connectionString)
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    });
else if (builder.Environment.IsProduction())
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
        var connectionStringTemplate = configuration.GetConnectionString("DefaultConnection");
        if (connectionStringTemplate is null)
            throw new Exception("Connection string is null");
        var connectionString = Environment.ExpandEnvironmentVariables(connectionStringTemplate);
        if (connectionString is null)
            throw new Exception("Connection string is null after expanding environment variables");
        options.UseMySQL(connectionString)
            .LogTo(Console.WriteLine, LogLevel.Error)
            .EnableDetailedErrors();
    });

// Configure Dependency Injection for Application Services

// Shared Bounded Context Injections 
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Reporting Bounded Context Injections
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IReportQueryServices, ReportQueryService>();
builder.Services.AddScoped<IReportCommandService, ReportCommandService>();

// Dashboard Bounded Context Injections
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IDashboardQueryService, DashboardQueryService>();
builder.Services.AddScoped<IDashboardCommandService, DashboardCommandService>();

var app = builder.Build();

//Verify Database Objects are created
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();