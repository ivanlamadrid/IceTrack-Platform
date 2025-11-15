using System.Net.Mime;
using IceTrackPlatform.API.Dashboard.Domain.Model.Queries;
using IceTrackPlatform.API.Dashboard.Domain.Services;
using IceTrackPlatform.API.Dashboard.Interfaces.REST.Resources;
using IceTrackPlatform.API.Dashboard.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IceTrackPlatform.API.Dashboard.Interfaces.REST;

/// <summary>
/// REST API controller for Dashboard operations.
/// Provides endpoints for creating, reading, and updating dashboards.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Dashboard")]
public class DashboardController(
    IDashboardCommandService commandService,
    IDashboardQueryService queryService) : ControllerBase
{
    /// <summary>
    /// Creates a new dashboard.
    /// </summary>
    /// <param name="resource">The dashboard creation data.</param>
    /// <returns>The created dashboard.</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create a new Dashboard",
        Description = "Creates a new dashboard for a user with the specified configuration",
        OperationId = "CreateDashboard")]
    [SwaggerResponse(201, "Dashboard created successfully", typeof(DashboardResource))]
    [SwaggerResponse(400, "Invalid input or validation error")]
    [SwaggerResponse(409, "Dashboard with this name already exists for the user")]
    public async Task<IActionResult> CreateDashboard([FromBody] CreateDashboardResource resource)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

        var command = CreateDashboardCommandFromResourceAssembler.ToCommandFromResource(resource);

        try
        {
            var result = await commandService.Handle(command);
            if (result is null) return BadRequest();

            return CreatedAtAction(
                nameof(GetDashboardById),
                new { id = result.Id },
                DashboardResourceFromEntityAssembler.ToResourceFromEntity(result));
        }
        catch (Exception e) when (e.Message.Contains("already exists"))
        {
            return Conflict(new { message = e.Message });
        }
        catch (Exception e) when (e.Message.Contains("does not exist"))
        {
            return BadRequest(new { message = e.Message });
        }
        catch (Exception e)
        {
            return BadRequest(new { message = e.Message });
        }
    }

    /// <summary>
    /// Gets a dashboard by its ID.
    /// </summary>
    /// <param name="id">The ID of the dashboard.</param>
    /// <returns>The dashboard if found.</returns>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Get Dashboard by ID",
        Description = "Retrieves a specific dashboard by its unique identifier",
        OperationId = "GetDashboardById")]
    [SwaggerResponse(200, "Dashboard found", typeof(DashboardResource))]
    [SwaggerResponse(404, "Dashboard not found")]
    public async Task<IActionResult> GetDashboardById(int id)
    {
        var query = new GetDashboardByIdQuery(id);
        var result = await queryService.Handle(query);

        if (result is null)
            return NotFound(new { message = $"Dashboard with ID {id} not found" });

        return Ok(DashboardResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    /// <summary>
    /// Gets all dashboards for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A collection of dashboards belonging to the user.</returns>
    [HttpGet("user/{userId}")]
    [SwaggerOperation(
        Summary = "Get Dashboards by User ID",
        Description = "Retrieves all dashboards belonging to a specific user",
        OperationId = "GetDashboardsByUserId")]
    [SwaggerResponse(200, "Dashboards retrieved successfully", typeof(IEnumerable<DashboardResource>))]
    public async Task<IActionResult> GetDashboardsByUserId(int userId)
    {
        var query = new GetDashboardsByUserIdQuery(userId);
        var result = await queryService.Handle(query);

        var resources = result.Select(DashboardResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    /// <summary>
    /// Gets all dashboards in the system.
    /// </summary>
    /// <returns>A collection of all dashboards.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get All Dashboards",
        Description = "Retrieves all dashboards in the system",
        OperationId = "GetAllDashboards")]
    [SwaggerResponse(200, "Dashboards retrieved successfully", typeof(IEnumerable<DashboardResource>))]
    public async Task<IActionResult> GetAllDashboards()
    {
        var query = new GetAllDashboardsQuery();
        var result = await queryService.Handle(query);

        var resources = result.Select(DashboardResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    /// <summary>
    /// Updates an existing dashboard.
    /// </summary>
    /// <param name="id">The ID of the dashboard to update.</param>
    /// <param name="resource">The updated dashboard data.</param>
    /// <returns>The updated dashboard.</returns>
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Update Dashboard",
        Description = "Updates an existing dashboard's information",
        OperationId = "UpdateDashboard")]
    [SwaggerResponse(200, "Dashboard updated successfully", typeof(DashboardResource))]
    [SwaggerResponse(404, "Dashboard not found")]
    [SwaggerResponse(400, "Invalid input")]
    public async Task<IActionResult> UpdateDashboard(int id, [FromBody] UpdateDashboardResource resource)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

        var command = UpdateDashboardCommandFromResourceAssembler.ToCommandFromResource(id, resource);

        try
        {
            var result = await commandService.Handle(command);
            if (result is null)
                return NotFound(new { message = $"Dashboard with ID {id} not found" });

            return Ok(DashboardResourceFromEntityAssembler.ToResourceFromEntity(result));
        }
        catch (Exception e)
        {
            return BadRequest(new { message = e.Message });
        }
    }
}
