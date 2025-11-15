namespace IceTrackPlatform.API.Dashboard.Domain.Model.Commands;

/// <summary>
/// Command to update an existing dashboard.
/// </summary>
/// <param name="Id">The ID of the dashboard to update.</param>
/// <param name="Name">The new name of the dashboard.</param>
/// <param name="Description">The new description of the dashboard.</param>
/// <param name="IsActive">Whether the dashboard is active.</param>
public record UpdateDashboardCommand(
    int Id,
    string Name,
    string Description,
    bool IsActive);
