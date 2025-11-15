using IceTrackPlatform.API.Dashboard.Domain.Model.ValueObjects;

namespace IceTrackPlatform.API.Dashboard.Domain.Model.Commands;

/// <summary>
/// Command to create a new dashboard.
/// </summary>
/// <param name="UserId">The ID of the user who owns the dashboard.</param>
/// <param name="Name">The name of the dashboard.</param>
/// <param name="Type">The type of the dashboard.</param>
/// <param name="Description">The description of the dashboard.</param>
public record CreateDashboardCommand(
    int UserId,
    string Name,
    EDashboardType Type,
    string Description);
