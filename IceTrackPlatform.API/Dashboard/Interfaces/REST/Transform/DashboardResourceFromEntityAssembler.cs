using IceTrackPlatform.API.Dashboard.Interfaces.REST.Resources;

namespace IceTrackPlatform.API.Dashboard.Interfaces.REST.Transform;

/// <summary>
/// Assembler to transform Dashboard entity to DashboardResource.
/// </summary>
public static class DashboardResourceFromEntityAssembler
{
    /// <summary>
    /// Converts a Dashboard entity to a DashboardResource.
    /// </summary>
    /// <param name="entity">The entity to convert.</param>
    /// <returns>The corresponding resource.</returns>
    public static DashboardResource ToResourceFromEntity(Domain.Model.Aggregates.Dashboard entity)
    {
        return new DashboardResource(
            entity.Id,
            entity.UserId,
            entity.Name,
            entity.Type.ToString(),
            entity.Description,
            entity.IsActive);
    }
}
