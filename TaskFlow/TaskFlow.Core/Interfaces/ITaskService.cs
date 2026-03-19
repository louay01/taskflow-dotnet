using TaskFlow.Core.Entities;
using TaskFlow.Core.Enums;

namespace TaskFlow.Core.Interfaces;

public interface ITaskService
{
    Task<IEnumerable<TaskItem>> GetAllAsync(int projectId, string currentUserId, bool isAdmin, TaskItemStatus? status = null, string? assigneeId = null);
    Task<TaskItem> GetByIdAsync(int projectId, int taskId, string currentUserId, bool isAdmin);
    Task<TaskItem> CreateAsync(int projectId, TaskItem task, string createdById, string currentUserId, bool isAdmin);
    Task<TaskItem> UpdateAsync(int projectId, int taskId, TaskItem updated, string currentUserId, bool isAdmin);
    Task<TaskItem> PatchStatusAsync(int projectId, int taskId, TaskItemStatus status, string currentUserId, bool isAdmin);
    Task DeleteAsync(int projectId, int taskId, string currentUserId, bool isAdmin);
}
