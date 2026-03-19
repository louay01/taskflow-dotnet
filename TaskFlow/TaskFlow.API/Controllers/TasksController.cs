using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.API.DTOs.Tasks;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Enums;
using TaskFlow.Core.Interfaces;

namespace TaskFlow.API.Controllers;

/// <summary>Task CRUD within a project.</summary>
[Authorize]
[Route("api/projects/{projectId}/tasks")]
[ApiController]
public class TasksController : BaseController
{
    private readonly ITaskService _taskService;
    private readonly IMapper _mapper;

    public TasksController(ITaskService taskService, IMapper mapper)
    {
        _taskService = taskService;
        _mapper = mapper;
    }

    /// <summary>Get all tasks in a project. Supports ?status= and ?assigneeId= filters.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<TaskItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(int projectId, [FromQuery] TaskItemStatus? status, [FromQuery] string? assigneeId)
    {
        var tasks = await _taskService.GetAllAsync(projectId, GetCurrentUserId(), IsAdmin(), status, assigneeId);
        return Ok(_mapper.Map<List<TaskItemDto>>(tasks));
    }

    /// <summary>Get a task by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int projectId, int id)
    {
        var task = await _taskService.GetByIdAsync(projectId, id, GetCurrentUserId(), IsAdmin());
        return Ok(_mapper.Map<TaskItemDto>(task));
    }

    /// <summary>Create a task. Manager or Admin only.</summary>
    [HttpPost]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(int projectId, [FromBody] CreateTaskItemDto dto)
    {
        var task = _mapper.Map<TaskItem>(dto);
        var created = await _taskService.CreateAsync(projectId, task, GetCurrentUserId(), GetCurrentUserId(), IsAdmin());
        return CreatedAtAction(nameof(GetById), new { projectId, id = created.Id }, _mapper.Map<TaskItemDto>(created));
    }

    /// <summary>Fully update a task. Manager, Admin, or assignee.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int projectId, int id, [FromBody] UpdateTaskItemDto dto)
    {
        var updated = await _taskService.UpdateAsync(projectId, id, _mapper.Map<TaskItem>(dto), GetCurrentUserId(), IsAdmin());
        return Ok(_mapper.Map<TaskItemDto>(updated));
    }

    /// <summary>Update only the status of a task. Any project member.</summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> PatchStatus(int projectId, int id, [FromBody] PatchTaskStatusDto dto)
    {
        var updated = await _taskService.PatchStatusAsync(projectId, id, dto.Status, GetCurrentUserId(), IsAdmin());
        return Ok(_mapper.Map<TaskItemDto>(updated));
    }

    /// <summary>Delete a task. Manager or Admin only.</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int projectId, int id)
    {
        await _taskService.DeleteAsync(projectId, id, GetCurrentUserId(), IsAdmin());
        return NoContent();
    }
}
