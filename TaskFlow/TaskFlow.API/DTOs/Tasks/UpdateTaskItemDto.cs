using System.ComponentModel.DataAnnotations;
using TaskFlow.Core.Enums;

namespace TaskFlow.API.DTOs.Tasks;

public class UpdateTaskItemDto
{
    [Required, MaxLength(300)] public string Title { get; set; } = string.Empty;
    [MaxLength(2000)] public string Description { get; set; } = string.Empty;
    public TaskItemStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public string? AssigneeId { get; set; }
}

public class PatchTaskStatusDto
{
    [Required] public TaskItemStatus Status { get; set; }
}
