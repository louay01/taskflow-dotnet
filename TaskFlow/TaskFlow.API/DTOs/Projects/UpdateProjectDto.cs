using System.ComponentModel.DataAnnotations;
using TaskFlow.Core.Enums;

namespace TaskFlow.API.DTOs.Projects;

public class UpdateProjectDto
{
    [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(1000)] public string Description { get; set; } = string.Empty;
    public ProjectStatus Status { get; set; }
    public DateTime? DueDate { get; set; }
}
