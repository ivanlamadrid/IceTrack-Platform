using IceTrackPlatform.API.Dashboard.Domain.Model.Aggregates;
using IceTrackPlatform.API.Dashboard.Domain.Model.Commands;

namespace IceTrackPlatform.API.Dashboard.Domain.Services;

/// <summary>
/// Service interface for handling dashboard commands.
/// </summary>
public interface IDashboardCommandService
{
    /// <summary>
    /// Handles the creation of a new dashboard.
    /// </summary>
    /// <param name="command">The create dashboard command.</param>
    /// <returns>The created dashboard, or null if creation failed.</returns>
    Task<Dashboard?> Handle(CreateDashboardCommand command);

    /// <summary>
    /// Handles the update of an existing dashboard.
    /// </summary>
    /// <param name="command">The update dashboard command.</param>
    /// <returns>The updated dashboard, or null if update failed.</returns>
    Task<Dashboard?> Handle(UpdateDashboardCommand command);
}
