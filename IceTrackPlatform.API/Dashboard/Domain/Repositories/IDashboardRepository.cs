using IceTrackPlatform.API.Dashboard.Domain.Model.Aggregates;
using IceTrackPlatform.API.Shared.Domain.Repositories;

namespace IceTrackPlatform.API.Dashboard.Domain.Repositories;

/// <summary>
/// Repository interface for Dashboard aggregate.
/// Extends base repository with dashboard-specific operations.
/// </summary>
public interface IDashboardRepository : IBaseRepository<Dashboard>
{
    /// <summary>
    /// Finds all dashboards for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A collection of dashboards belonging to the user.</returns>
    Task<IEnumerable<Dashboard>> FindByUserIdAsync(int userId);

    /// <summary>
    /// Finds a dashboard by name and user ID.
    /// </summary>
    /// <param name="name">The name of the dashboard.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>The dashboard if found, otherwise null.</returns>
    Task<Dashboard?> FindByNameAndUserIdAsync(string name, int userId);
}
