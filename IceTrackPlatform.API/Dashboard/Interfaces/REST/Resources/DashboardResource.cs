namespace IceTrackPlatform.API.Dashboard.Interfaces.REST.Resources;

/// <summary>
/// Resource (DTO) representing a dashboard for API responses.
/// </summary>
/// <param name="Id">The unique identifier of the dashboard.</param>
/// <param name="UserId">The ID of the user who owns the dashboard.</param>
/// <param name="Name">The name of the dashboard.</param>
/// <param name="Type">The type of the dashboard.</param>
/// <param name="Description">The description of the dashboard.</param>
/// <param name="IsActive">Whether the dashboard is active.</param>
public record DashboardResource(
    int Id,
    int UserId,
    string Name,
    string Type,
    string Description,
    bool IsActive);
