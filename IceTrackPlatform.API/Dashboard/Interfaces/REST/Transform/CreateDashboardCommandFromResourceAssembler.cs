using IceTrackPlatform.API.Dashboard.Domain.Model.Commands;
using IceTrackPlatform.API.Dashboard.Domain.Model.ValueObjects;
using IceTrackPlatform.API.Dashboard.Interfaces.REST.Resources;

namespace IceTrackPlatform.API.Dashboard.Interfaces.REST.Transform;

/// <summary>
/// Assembler to transform CreateDashboardResource to CreateDashboardCommand.
/// </summary>
public static class CreateDashboardCommandFromResourceAssembler
{
    /// <summary>
    /// Converts a CreateDashboardResource to a CreateDashboardCommand.
    /// </summary>
    /// <param name="resource">The resource to convert.</param>
    /// <returns>The corresponding command.</returns>
    public static CreateDashboardCommand ToCommandFromResource(CreateDashboardResource resource)
    {
        return new CreateDashboardCommand(
            resource.UserId,
            resource.Name,
            Enum.Parse<EDashboardType>(resource.Type),
            resource.Description);
    }
}
