using TaskFlow.Core.Entities;

namespace TaskFlow.Core.Interfaces;

public interface ICommentService
{
    Task<IEnumerable<Comment>> GetByTaskAsync(int taskItemId, string currentUserId, bool isAdmin);
    Task<Comment> CreateAsync(int taskItemId, Comment comment, string authorId, string currentUserId, bool isAdmin);
    Task<Comment> UpdateAsync(int commentId, string content, string currentUserId, bool isAdmin);
    Task DeleteAsync(int commentId, string currentUserId, bool isAdmin, bool isManager);
}
