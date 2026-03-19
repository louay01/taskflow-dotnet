using Microsoft.EntityFrameworkCore;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Interfaces;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Infrastructure.Services;

public class ProjectService : IProjectService
{
    private readonly ApplicationDbContext _context;

    public ProjectService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Project>> GetAllAsync(string currentUserId, bool isAdmin)
    {
        var query = _context.Projects
            .Include(p => p.Owner)
            .Include(p => p.Members)
            .AsQueryable();

        if (!isAdmin)
        {
            query = query.Where(p =>
                p.OwnerId == currentUserId ||
                p.Members.Any(m => m.UserId == currentUserId));
        }

        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    public async Task<Project> GetByIdAsync(int id, string currentUserId, bool isAdmin)
    {
        var project = await _context.Projects
            .Include(p => p.Owner)
            .Include(p => p.Members).ThenInclude(m => m.User)
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Project {id} not found.");

        if (!isAdmin && project.OwnerId != currentUserId &&
            !project.Members.Any(m => m.UserId == currentUserId))
            throw new UnauthorizedAccessException("Access denied.");

        return project;
    }

    public async Task<Project> CreateAsync(Project project, string ownerId)
    {
        project.OwnerId = ownerId;
        project.CreatedAt = DateTime.UtcNow;

        _context.Projects.Add(project);

        // Auto-add owner as a member
        _context.ProjectMembers.Add(new ProjectMember
        {
            Project = project,
            UserId = ownerId,
        });

        await _context.SaveChangesAsync();
        return project;
    }

    public async Task<Project> UpdateAsync(int id, Project updated, string currentUserId, bool isAdmin)
    {
        var project = await _context.Projects.FindAsync(id)
            ?? throw new KeyNotFoundException($"Project {id} not found.");

        if (!isAdmin && project.OwnerId != currentUserId)
            throw new UnauthorizedAccessException("Only the project owner or admin can edit.");

        project.Name = updated.Name;
        project.Description = updated.Description;
        project.Status = updated.Status;
        project.DueDate = updated.DueDate;

        await _context.SaveChangesAsync();
        return project;
    }

    public async Task DeleteAsync(int id, bool isAdmin)
    {
        if (!isAdmin)
            throw new UnauthorizedAccessException("Only admins can delete projects.");

        var project = await _context.Projects.FindAsync(id)
            ?? throw new KeyNotFoundException($"Project {id} not found.");

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
    }

    public async Task AddMemberAsync(int projectId, string userId, string currentUserId, bool isAdmin)
    {
        var project = await _context.Projects.FindAsync(projectId)
            ?? throw new KeyNotFoundException($"Project {projectId} not found.");

        if (!isAdmin && project.OwnerId != currentUserId)
            throw new UnauthorizedAccessException("Only the project owner or admin can add members.");

        var alreadyMember = await _context.ProjectMembers
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);
        if (alreadyMember)
            throw new InvalidOperationException("User is already a member.");

        _context.ProjectMembers.Add(new ProjectMember { ProjectId = projectId, UserId = userId });
        await _context.SaveChangesAsync();
    }

    public async Task RemoveMemberAsync(int projectId, string userId, string currentUserId, bool isAdmin)
    {
        var project = await _context.Projects.FindAsync(projectId)
            ?? throw new KeyNotFoundException($"Project {projectId} not found.");

        if (!isAdmin && project.OwnerId != currentUserId)
            throw new UnauthorizedAccessException("Only the project owner or admin can remove members.");

        if (project.OwnerId == userId)
            throw new InvalidOperationException("Cannot remove the project owner.");

        var member = await _context.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId)
            ?? throw new KeyNotFoundException("Member not found.");

        _context.ProjectMembers.Remove(member);
        await _context.SaveChangesAsync();
    }
}
