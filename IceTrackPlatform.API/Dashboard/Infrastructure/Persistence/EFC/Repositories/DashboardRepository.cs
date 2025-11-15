using IceTrackPlatform.API.Dashboard.Domain.Model.Aggregates;
using IceTrackPlatform.API.Dashboard.Domain.Repositories;
using IceTrackPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using IceTrackPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IceTrackPlatform.API.Dashboard.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// Repository implementation for Dashboard aggregate.
/// Extends base repository with dashboard-specific query methods.
/// </summary>
public class DashboardRepository(AppDbContext context)
    : BaseRepository<Dashboard>(context), IDashboardRepository
{
    /// <summary>
    /// Finds all dashboards for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A collection of dashboards belonging to the user.</returns>
    public async Task<IEnumerable<Dashboard>> FindByUserIdAsync(int userId)
    {
        return await Context.Set<Dashboard>()
            .Where(d => d.UserId == userId)
            .ToListAsync();
    }

    /// <summary>
    /// Finds a dashboard by name and user ID.
    /// Used to check for duplicate dashboard names for the same user.
    /// </summary>
    /// <param name="name">The name of the dashboard.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>The dashboard if found, otherwise null.</returns>
    public async Task<Dashboard?> FindByNameAndUserIdAsync(string name, int userId)
    {
        return await Context.Set<Dashboard>()
            .FirstOrDefaultAsync(d => d.UserId == userId && d.Name == name);
    }
}
