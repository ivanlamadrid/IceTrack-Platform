using IceTrackPlatform.API.Dashboard.Domain.Model.Aggregates;
using IceTrackPlatform.API.Dashboard.Domain.Model.Commands;
using IceTrackPlatform.API.Dashboard.Domain.Repositories;
using IceTrackPlatform.API.Dashboard.Domain.Services;
using IceTrackPlatform.API.IAM.Interfaces.ACL;
using IceTrackPlatform.API.Shared.Domain.Repositories;

namespace IceTrackPlatform.API.Dashboard.Application.Internal.CommandServices;

/// <summary>
/// Service for handling dashboard commands.
/// Implements business logic and validation for dashboard operations.
/// </summary>
public class DashboardCommandService(
    IDashboardRepository dashboardRepository,
    IIamContextFacade iamContextFacade,
    IUnitOfWork unitOfWork) : IDashboardCommandService
{
    /// <summary>
    /// Handles the creation of a new dashboard.
    /// Validates user existence via IAM ACL and checks for duplicate names.
    /// </summary>
    /// <param name="command">The create dashboard command.</param>
    /// <returns>The created dashboard, or null if creation failed.</returns>
    /// <exception cref="Exception">Thrown when user doesn't exist or dashboard name is duplicated.</exception>
    public async Task<Dashboard?> Handle(CreateDashboardCommand command)
    {
        // Validate user exists using IAM ACL
        var username = await iamContextFacade.FetchUsernameByUserId(command.UserId);
        if (string.IsNullOrEmpty(username))
            throw new Exception($"User with ID {command.UserId} does not exist");

        // Validate business rule: no duplicate dashboard names for the same user
        var existing = await dashboardRepository.FindByNameAndUserIdAsync(command.Name, command.UserId);
        if (existing != null)
            throw new Exception($"Dashboard with name '{command.Name}' already exists for this user");

        // Create dashboard aggregate
        var dashboard = new Dashboard(command);

        // Persist to database
        await dashboardRepository.AddAsync(dashboard);
        await unitOfWork.CompleteAsync();

        return dashboard;
    }

    /// <summary>
    /// Handles the update of an existing dashboard.
    /// </summary>
    /// <param name="command">The update dashboard command.</param>
    /// <returns>The updated dashboard, or null if dashboard not found.</returns>
    public async Task<Dashboard?> Handle(UpdateDashboardCommand command)
    {
        var dashboard = await dashboardRepository.FindByIdAsync(command.Id);
        if (dashboard == null)
            return null;

        // Update dashboard using domain method
        dashboard.Update(command.Name, command.Description, command.IsActive);

        dashboardRepository.Update(dashboard);
        await unitOfWork.CompleteAsync();

        return dashboard;
    }
}
