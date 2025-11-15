using EntityFrameworkCore.CreatedUpdatedDate.Contracts;

namespace IceTrackPlatform.API.Dashboard.Domain.Model.Aggregates;

/// <summary>
/// Represents the audit information for a dashboard.
/// This partial class extends Dashboard with auditing capabilities.
/// </summary>
public partial class Dashboard : IEntityWithCreatedUpdatedDate
{
    /// <summary>
    /// Gets or sets the date when the dashboard was created.
    /// </summary>
    public DateTimeOffset? CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the date when the dashboard was last updated.
    /// </summary>
    public DateTimeOffset? UpdatedDate { get; set; }
}
