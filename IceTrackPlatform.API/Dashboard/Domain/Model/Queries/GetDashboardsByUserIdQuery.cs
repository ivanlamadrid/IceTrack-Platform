namespace IceTrackPlatform.API.Dashboard.Domain.Model.Queries;

/// <summary>
/// Query to get all dashboards for a specific user.
/// </summary>
/// <param name="UserId">The ID of the user.</param>
public record GetDashboardsByUserIdQuery(int UserId);
