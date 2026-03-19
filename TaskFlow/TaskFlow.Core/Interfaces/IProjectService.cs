using TaskFlow.Core.Entities;
using TaskFlow.Core.Enums;

namespace TaskFlow.Core.Interfaces;

public interface IProjectService
{
    Task<IEnumerable<Project>> GetAllAsync(string currentUserId, bool isAdmin);
    Task<Project> GetByIdAsync(int id, string currentUserId, bool isAdmin);
    Task<Project> CreateAsync(Project project, string ownerId);
    Task<Project> UpdateAsync(int id, Project updated, string currentUserId, bool isAdmin);
    Task DeleteAsync(int id, bool isAdmin);
    Task AddMemberAsync(int projectId, string userId, string currentUserId, bool isAdmin);
    Task RemoveMemberAsync(int projectId, string userId, string currentUserId, bool isAdmin);
}
