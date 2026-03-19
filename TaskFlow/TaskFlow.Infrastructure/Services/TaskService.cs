using Microsoft.EntityFrameworkCore;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Enums;
using TaskFlow.Core.Interfaces;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;

    public TaskService(ApplicationDbContext context)
    {
        _context = context;
    }

    private async Task EnsureMemberAccessAsync(int projectId, string currentUserId, bool isAdmin)
    {
        if (isAdmin) return;
        var isMember = await _context.ProjectMembers
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == currentUserId);
        if (!isMember)
            throw new UnauthorizedAccessException("You are not a member of this project.");
    }

    public async Task<IEnumerable<TaskItem>> GetAllAsync(int projectId, string currentUserId, bool isAdmin,
        TaskItemStatus? status = null, string? assigneeId = null)
    {
        await EnsureMemberAccessAsync(projectId, currentUserId, isAdmin);

        var query = _context.TaskItems
            .Include(t => t.Assignee)
            .Include(t => t.CreatedBy)
            .Where(t => t.ProjectId == projectId)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);
        if (!string.IsNullOrEmpty(assigneeId))
            query = query.Where(t => t.AssigneeId == assigneeId);

        return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
    }

    public async Task<TaskItem> GetByIdAsync(int projectId, int taskId, string currentUserId, bool isAdmin)
    {
        await EnsureMemberAccessAsync(projectId, currentUserId, isAdmin);

        return await _context.TaskItems
            .Include(t => t.Assignee)
            .Include(t => t.CreatedBy)
            .Include(t => t.Comments).ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId)
            ?? throw new KeyNotFoundException($"Task {taskId} not found.");
    }

    public async Task<TaskItem> CreateAsync(int projectId, TaskItem task, string createdById, string currentUserId, bool isAdmin)
    {
        if (!isAdmin)
        {
            var isManager = await _context.ProjectMembers
                .AnyAsync(m => m.ProjectId == projectId && m.UserId == currentUserId);
            if (!isManager)
                throw new UnauthorizedAccessException("Access denied.");
        }

        task.ProjectId = projectId;
        task.CreatedById = createdById;
        task.CreatedAt = DateTime.UtcNow;

        _context.TaskItems.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<TaskItem> UpdateAsync(int projectId, int taskId, TaskItem updated, string currentUserId, bool isAdmin)
    {
        await EnsureMemberAccessAsync(projectId, currentUserId, isAdmin);

        var task = await _context.TaskItems
            .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId)
            ?? throw new KeyNotFoundException($"Task {taskId} not found.");

        // Members can only change status; full update requires Manager/Admin
        if (!isAdmin && task.AssigneeId != currentUserId)
        {
            var isMemberOnly = !await IsManagerOrOwnerAsync(projectId, currentUserId);
            if (isMemberOnly)
                throw new UnauthorizedAccessException("Only managers, admins, or the assignee can fully update a task.");
        }

        task.Title = updated.Title;
        task.Description = updated.Description;
        task.Status = updated.Status;
        task.Priority = updated.Priority;
        task.DueDate = updated.DueDate;
        task.AssigneeId = updated.AssigneeId;

        if (task.Status == TaskItemStatus.Done && task.CompletedAt == null)
            task.CompletedAt = DateTime.UtcNow;
        else if (task.Status != TaskItemStatus.Done)
            task.CompletedAt = null;

        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<TaskItem> PatchStatusAsync(int projectId, int taskId, TaskItemStatus status, string currentUserId, bool isAdmin)
    {
        await EnsureMemberAccessAsync(projectId, currentUserId, isAdmin);

        var task = await _context.TaskItems
            .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId)
            ?? throw new KeyNotFoundException($"Task {taskId} not found.");

        task.Status = status;
        if (status == TaskItemStatus.Done)
            task.CompletedAt = DateTime.UtcNow;
        else
            task.CompletedAt = null;

        await _context.SaveChangesAsync();
        return task;
    }

    public async Task DeleteAsync(int projectId, int taskId, string currentUserId, bool isAdmin)
    {
        if (!isAdmin && !await IsManagerOrOwnerAsync(projectId, currentUserId))
            throw new UnauthorizedAccessException("Only managers, project owners, or admins can delete tasks.");

        var task = await _context.TaskItems
            .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId)
            ?? throw new KeyNotFoundException($"Task {taskId} not found.");

        _context.TaskItems.Remove(task);
        await _context.SaveChangesAsync();
    }

    private async Task<bool> IsManagerOrOwnerAsync(int projectId, string userId)
    {
        return await _context.Projects.AnyAsync(p => p.Id == projectId && p.OwnerId == userId);
    }
}
