namespace TaskFlow.Core.Entities;

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int TaskItemId { get; set; }
    public string AuthorId { get; set; } = string.Empty;

    public TaskItem TaskItem { get; set; } = null!;
    public ApplicationUser Author { get; set; } = null!;
}
