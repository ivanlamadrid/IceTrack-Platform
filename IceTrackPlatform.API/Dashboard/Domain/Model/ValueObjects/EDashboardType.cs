namespace IceTrackPlatform.API.Dashboard.Domain.Model.ValueObjects;

/// <summary>
/// Defines the possible types for a dashboard.
/// </summary>
public enum EDashboardType
{
    /// <summary>
    /// Dashboard for analytics and statistics visualization.
    /// </summary>
    Analytics,

    /// <summary>
    /// Dashboard for real-time monitoring of equipment.
    /// </summary>
    Monitoring,

    /// <summary>
    /// Dashboard focused on reports visualization.
    /// </summary>
    Reports,

    /// <summary>
    /// Custom dashboard type defined by the user.
    /// </summary>
    Custom
}
