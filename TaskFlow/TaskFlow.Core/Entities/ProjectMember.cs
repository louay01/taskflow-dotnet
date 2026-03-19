namespace TaskFlow.Core.Entities;

public class ProjectMember
{
    public int ProjectId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public Project Project { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
