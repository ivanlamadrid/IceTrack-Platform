using IceTrackPlatform.API.Dashboard.Domain.Model.Aggregates;
using IceTrackPlatform.API.Dashboard.Domain.Model.Queries;
using IceTrackPlatform.API.Dashboard.Domain.Repositories;
using IceTrackPlatform.API.Dashboard.Domain.Services;

namespace IceTrackPlatform.API.Dashboard.Application.Internal.QueryServices;

/// <summary>
/// Service for handling dashboard queries.
/// Implements read operations for dashboards.
/// </summary>
public class DashboardQueryService(IDashboardRepository dashboardRepository) : IDashboardQueryService
{
    /// <summary>
    /// Handles retrieving a dashboard by its ID.
    /// </summary>
    /// <param name="query">The query containing the dashboard ID.</param>
    /// <returns>The dashboard if found, otherwise null.</returns>
    public async Task<Dashboard?> Handle(GetDashboardByIdQuery query)
    {
        return await dashboardRepository.FindByIdAsync(query.Id);
    }

    /// <summary>
    /// Handles retrieving all dashboards for a specific user.
    /// </summary>
    /// <param name="query">The query containing the user ID.</param>
    /// <returns>A collection of dashboards belonging to the user.</returns>
    public async Task<IEnumerable<Dashboard>> Handle(GetDashboardsByUserIdQuery query)
    {
        return await dashboardRepository.FindByUserIdAsync(query.UserId);
    }

    /// <summary>
    /// Handles retrieving all dashboards in the system.
    /// </summary>
    /// <param name="query">The query to get all dashboards.</param>
    /// <returns>A collection of all dashboards.</returns>
    public async Task<IEnumerable<Dashboard>> Handle(GetAllDashboardsQuery query)
    {
        return await dashboardRepository.ListAsync();
    }
}
