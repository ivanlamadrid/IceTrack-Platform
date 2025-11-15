using IceTrackPlatform.API.Dashboard.Domain.Model.Aggregates;
using IceTrackPlatform.API.Dashboard.Domain.Model.Queries;

namespace IceTrackPlatform.API.Dashboard.Domain.Services;

/// <summary>
/// Service interface for handling dashboard queries.
/// </summary>
public interface IDashboardQueryService
{
    /// <summary>
    /// Handles retrieving a dashboard by its ID.
    /// </summary>
    /// <param name="query">The query containing the dashboard ID.</param>
    /// <returns>The dashboard if found, otherwise null.</returns>
    Task<Dashboard?> Handle(GetDashboardByIdQuery query);

    /// <summary>
    /// Handles retrieving all dashboards for a specific user.
    /// </summary>
    /// <param name="query">The query containing the user ID.</param>
    /// <returns>A collection of dashboards belonging to the user.</returns>
    Task<IEnumerable<Dashboard>> Handle(GetDashboardsByUserIdQuery query);

    /// <summary>
    /// Handles retrieving all dashboards in the system.
    /// </summary>
    /// <param name="query">The query to get all dashboards.</param>
    /// <returns>A collection of all dashboards.</returns>
    Task<IEnumerable<Dashboard>> Handle(GetAllDashboardsQuery query);
}
