using IceTrackPlatform.API.Dashboard.Domain.Model.Commands;
using IceTrackPlatform.API.Dashboard.Interfaces.REST.Resources;

namespace IceTrackPlatform.API.Dashboard.Interfaces.REST.Transform;

/// <summary>
/// Assembler to transform UpdateDashboardResource to UpdateDashboardCommand.
/// </summary>
public static class UpdateDashboardCommandFromResourceAssembler
{
    /// <summary>
    /// Converts an UpdateDashboardResource to an UpdateDashboardCommand.
    /// </summary>
    /// <param name="id">The ID of the dashboard to update.</param>
    /// <param name="resource">The resource to convert.</param>
    /// <returns>The corresponding command.</returns>
    public static UpdateDashboardCommand ToCommandFromResource(int id, UpdateDashboardResource resource)
    {
        return new UpdateDashboardCommand(
            id,
            resource.Name,
            resource.Description,
            resource.IsActive);
    }
}
