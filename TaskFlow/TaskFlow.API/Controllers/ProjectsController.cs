using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.API.DTOs.Projects;
using TaskFlow.API.DTOs.Users;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Interfaces;

namespace TaskFlow.API.Controllers;

/// <summary>Project CRUD and membership management.</summary>
[Authorize]
public class ProjectsController : BaseController
{
    private readonly IProjectService _projectService;
    private readonly IMapper _mapper;

    public ProjectsController(IProjectService projectService, IMapper mapper)
    {
        _projectService = projectService;
        _mapper = mapper;
    }

    /// <summary>Get all projects. Admins see all; others see only their own.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var projects = await _projectService.GetAllAsync(GetCurrentUserId(), IsAdmin());
        return Ok(_mapper.Map<List<ProjectDto>>(projects));
    }

    /// <summary>Get a project by ID. Must be a member or admin.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var project = await _projectService.GetByIdAsync(id, GetCurrentUserId(), IsAdmin());
        return Ok(_mapper.Map<ProjectDto>(project));
    }

    /// <summary>Create a new project. Manager or Admin only.</summary>
    [HttpPost]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
    {
        var project = _mapper.Map<Project>(dto);
        var created = await _projectService.CreateAsync(project, GetCurrentUserId());
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, _mapper.Map<ProjectDto>(created));
    }

    /// <summary>Update a project. Owner or Admin only.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectDto dto)
    {
        var updated = await _projectService.UpdateAsync(id, _mapper.Map<Project>(dto), GetCurrentUserId(), IsAdmin());
        return Ok(_mapper.Map<ProjectDto>(updated));
    }

    /// <summary>Delete a project. Admin only.</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
    {
        await _projectService.DeleteAsync(id, IsAdmin());
        return NoContent();
    }

    /// <summary>Add a member to a project. Owner or Admin only.</summary>
    [HttpPost("{id}/members")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AddMember(int id, [FromBody] AddMemberDto dto)
    {
        await _projectService.AddMemberAsync(id, dto.UserId, GetCurrentUserId(), IsAdmin());
        return NoContent();
    }

    /// <summary>Remove a member from a project. Owner or Admin only.</summary>
    [HttpDelete("{id}/members/{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveMember(int id, string userId)
    {
        await _projectService.RemoveMemberAsync(id, userId, GetCurrentUserId(), IsAdmin());
        return NoContent();
    }
}
