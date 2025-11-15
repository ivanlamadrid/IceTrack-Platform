using IceTrackPlatform.API.Dashboard.Domain.Model.Commands;
using IceTrackPlatform.API.Dashboard.Domain.Model.ValueObjects;

namespace IceTrackPlatform.API.Dashboard.Domain.Model.Aggregates;

/// <summary>
/// Represents a dashboard aggregate root.
/// A dashboard is a personalized view for a user to visualize data.
/// </summary>
public partial class Dashboard
{
    /// <summary>
    /// Gets the unique identifier of the dashboard.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the ID of the user who owns this dashboard.
    /// </summary>
    public int UserId { get; private set; }

    /// <summary>
    /// Gets the name of the dashboard.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the type of the dashboard.
    /// </summary>
    public EDashboardType Type { get; private set; }

    /// <summary>
    /// Gets the description of the dashboard.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Gets whether the dashboard is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Initializes a new instance of the Dashboard class.
    /// </summary>
    /// <param name="userId">The ID of the user who owns the dashboard.</param>
    /// <param name="name">The name of the dashboard.</param>
    /// <param name="type">The type of the dashboard.</param>
    /// <param name="description">The description of the dashboard.</param>
    /// <param name="isActive">Whether the dashboard is active.</param>
    public Dashboard(int userId, string name, EDashboardType type, string description, bool isActive = true)
    {
        UserId = userId;
        Name = name;
        Type = type;
        Description = description;
        IsActive = isActive;
    }

    /// <summary>
    /// Creates a new Dashboard from a CreateDashboardCommand.
    /// </summary>
    /// <param name="command">The CreateDashboardCommand.</param>
    public Dashboard(CreateDashboardCommand command)
    {
        UserId = command.UserId;
        Name = command.Name;
        Type = command.Type;
        Description = command.Description;
        IsActive = true;
    }

    /// <summary>
    /// Updates the dashboard information.
    /// </summary>
    /// <param name="name">The new name.</param>
    /// <param name="description">The new description.</param>
    /// <param name="isActive">The new active status.</param>
    public void Update(string name, string description, bool isActive)
    {
        Name = name;
        Description = description;
        IsActive = isActive;
    }

    /// <summary>
    /// Deactivates the dashboard.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Activates the dashboard.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }
}
