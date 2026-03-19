using System.ComponentModel.DataAnnotations;

namespace TaskFlow.API.DTOs.Comments;

public class CommentDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public int TaskItemId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateCommentDto
{
    [Required, MaxLength(2000)] public string Content { get; set; } = string.Empty;
}

public class UpdateCommentDto
{
    [Required, MaxLength(2000)] public string Content { get; set; } = string.Empty;
}
