using System.ComponentModel.DataAnnotations;

namespace IceTrackPlatform.API.Dashboard.Interfaces.REST.Resources;

/// <summary>
/// Resource (DTO) for creating a new dashboard.
/// </summary>
/// <param name="UserId">The ID of the user who owns the dashboard.</param>
/// <param name="Name">The name of the dashboard.</param>
/// <param name="Type">The type of the dashboard (Analytics, Monitoring, Reports, Custom).</param>
/// <param name="Description">The description of the dashboard.</param>
public record CreateDashboardResource(
    [Required] int UserId,
    [Required] string Name,
    [Required] string Type,
    [Required] string Description);
