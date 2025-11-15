using IceTrackPlatform.API.Dashboard.Domain.Model.Aggregates;
using IceTrackPlatform.API.Reporting.Domain.Model.Aggregates;
using IceTrackPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;

namespace IceTrackPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;

using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Microsoft.EntityFrameworkCore;


/// <summary>
///     Application database context for the Learning Center Platform
/// </summary>
/// <param name="options">
///     The options for the database context
/// </param>
public class AppDbContext(DbContextOptions options) : DbContext(options)
{
   /// <summary>
   ///     On configuring the database context
   /// </summary>
   /// <remarks>
   ///     This method is used to configure the database context.
   ///     It also adds the created and updated date interceptor to the database context.
   /// </remarks>
   /// <param name="builder">
   ///     The option builder for the database context
   /// </param>
   protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.AddCreatedUpdatedInterceptor();
        base.OnConfiguring(builder);
    }

   /// <summary>
   ///     On creating the database model
   /// </summary>
   /// <remarks>
   ///     This method is used to create the database model for the application.
   /// </remarks>
   /// <param name="builder">
   ///     The model builder for the database context
   /// </param>
   protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // IAM Context
        // Apply IAM configuration when available
        // builder.ApplyIAMConfiguration();

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
        builder.Entity<Dashboard>().Property(d => d.Description).IsRequired();
        builder.Entity<Dashboard>().Property(d => d.IsActive).IsRequired();

        // General Naming Convention for the database objects
        builder.UseSnakeCaseNamingConvention();
    }
}