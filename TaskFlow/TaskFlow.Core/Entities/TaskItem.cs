using TaskFlow.Core.Enums;

namespace TaskFlow.Core.Entities;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }

    public int ProjectId { get; set; }
    public string? AssigneeId { get; set; }
    public string CreatedById { get; set; } = string.Empty;

    public Project Project { get; set; } = null!;
    public ApplicationUser? Assignee { get; set; }
    public ApplicationUser CreatedBy { get; set; } = null!;
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
