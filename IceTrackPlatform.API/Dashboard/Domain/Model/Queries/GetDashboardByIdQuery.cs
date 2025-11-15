namespace IceTrackPlatform.API.Dashboard.Domain.Model.Queries;

/// <summary>
/// Query to get a dashboard by its ID.
/// </summary>
/// <param name="Id">The ID of the dashboard.</param>
public record GetDashboardByIdQuery(int Id);
