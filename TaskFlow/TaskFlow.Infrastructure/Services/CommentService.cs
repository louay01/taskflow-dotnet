using Microsoft.EntityFrameworkCore;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Interfaces;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Infrastructure.Services;

public class CommentService : ICommentService
{
    private readonly ApplicationDbContext _context;

    public CommentService(ApplicationDbContext context)
    {
        _context = context;
    }

    private async Task EnsureTaskAccessAsync(int taskItemId, string currentUserId, bool isAdmin)
    {
        if (isAdmin) return;
        var task = await _context.TaskItems
            .Include(t => t.Project).ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(t => t.Id == taskItemId)
            ?? throw new KeyNotFoundException($"Task {taskItemId} not found.");

        if (!task.Project.Members.Any(m => m.UserId == currentUserId))
            throw new UnauthorizedAccessException("You are not a member of this project.");
    }

    public async Task<IEnumerable<Comment>> GetByTaskAsync(int taskItemId, string currentUserId, bool isAdmin)
    {
        await EnsureTaskAccessAsync(taskItemId, currentUserId, isAdmin);

        return await _context.Comments
            .Include(c => c.Author)
            .Where(c => c.TaskItemId == taskItemId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Comment> CreateAsync(int taskItemId, Comment comment, string authorId, string currentUserId, bool isAdmin)
    {
        await EnsureTaskAccessAsync(taskItemId, currentUserId, isAdmin);

        comment.TaskItemId = taskItemId;
        comment.AuthorId = authorId;
        comment.CreatedAt = DateTime.UtcNow;

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task<Comment> UpdateAsync(int commentId, string content, string currentUserId, bool isAdmin)
    {
        var comment = await _context.Comments.FindAsync(commentId)
            ?? throw new KeyNotFoundException($"Comment {commentId} not found.");

        if (!isAdmin && comment.AuthorId != currentUserId)
            throw new UnauthorizedAccessException("You can only edit your own comments.");

        comment.Content = content;
        comment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task DeleteAsync(int commentId, string currentUserId, bool isAdmin, bool isManager)
    {
        var comment = await _context.Comments.FindAsync(commentId)
            ?? throw new KeyNotFoundException($"Comment {commentId} not found.");

        if (!isAdmin && !isManager && comment.AuthorId != currentUserId)
            throw new UnauthorizedAccessException("You can only delete your own comments.");

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
    }
}
