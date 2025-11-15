using System.ComponentModel.DataAnnotations;

namespace IceTrackPlatform.API.Dashboard.Interfaces.REST.Resources;

/// <summary>
/// Resource (DTO) for updating an existing dashboard.
/// </summary>
/// <param name="Name">The new name of the dashboard.</param>
/// <param name="Description">The new description of the dashboard.</param>
/// <param name="IsActive">Whether the dashboard is active.</param>
public record UpdateDashboardResource(
    [Required] string Name,
    [Required] string Description,
    [Required] bool IsActive);
