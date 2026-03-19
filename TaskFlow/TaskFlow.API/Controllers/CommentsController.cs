using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.API.DTOs.Comments;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Interfaces;

namespace TaskFlow.API.Controllers;

/// <summary>Comments on tasks.</summary>
[Authorize]
[Route("api/tasks/{taskId}/comments")]
[ApiController]
public class CommentsController : BaseController
{
    private readonly ICommentService _commentService;
    private readonly IMapper _mapper;

    public CommentsController(ICommentService commentService, IMapper mapper)
    {
        _commentService = commentService;
        _mapper = mapper;
    }

    /// <summary>Get all comments for a task.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CommentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(int taskId)
    {
        var comments = await _commentService.GetByTaskAsync(taskId, GetCurrentUserId(), IsAdmin());
        return Ok(_mapper.Map<List<CommentDto>>(comments));
    }

    /// <summary>Add a comment to a task. Any project member.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(int taskId, [FromBody] CreateCommentDto dto)
    {
        var comment = _mapper.Map<Comment>(dto);
        var created = await _commentService.CreateAsync(taskId, comment, GetCurrentUserId(), GetCurrentUserId(), IsAdmin());
        return CreatedAtAction(nameof(GetAll), new { taskId }, _mapper.Map<CommentDto>(created));
    }

    /// <summary>Edit a comment. Author or Admin only.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int taskId, int id, [FromBody] UpdateCommentDto dto)
    {
        var updated = await _commentService.UpdateAsync(id, dto.Content, GetCurrentUserId(), IsAdmin());
        return Ok(_mapper.Map<CommentDto>(updated));
    }

    /// <summary>Delete a comment. Author, Manager, or Admin.</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int taskId, int id)
    {
        await _commentService.DeleteAsync(id, GetCurrentUserId(), IsAdmin(), IsManager());
        return NoContent();
    }
}
