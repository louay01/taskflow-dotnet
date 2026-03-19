using AutoMapper;
using TaskFlow.API.DTOs.Auth;
using TaskFlow.API.DTOs.Comments;
using TaskFlow.API.DTOs.Projects;
using TaskFlow.API.DTOs.Tasks;
using TaskFlow.API.DTOs.Users;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Interfaces;

namespace TaskFlow.API.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Auth
        CreateMap<AuthResult, AuthResponseDto>()
            .ForMember(d => d.User, o => o.MapFrom(s => s.User));
        CreateMap<UserInfo, UserResponseDto>();

        // Projects
        CreateMap<Project, ProjectDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.OwnerName, o => o.MapFrom(s => s.Owner != null ? $"{s.Owner.FirstName} {s.Owner.LastName}" : ""))
            .ForMember(d => d.Members, o => o.MapFrom(s => s.Members));
        CreateMap<ProjectMember, ProjectMemberDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.User != null ? $"{s.User.FirstName} {s.User.LastName}" : ""))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.User != null ? s.User.Email : ""));
        CreateMap<CreateProjectDto, Project>();
        CreateMap<UpdateProjectDto, Project>();

        // Tasks
        CreateMap<TaskItem, TaskItemDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Priority, o => o.MapFrom(s => s.Priority.ToString()))
            .ForMember(d => d.AssigneeName, o => o.MapFrom(s => s.Assignee != null ? $"{s.Assignee.FirstName} {s.Assignee.LastName}" : null))
            .ForMember(d => d.CreatedByName, o => o.MapFrom(s => s.CreatedBy != null ? $"{s.CreatedBy.FirstName} {s.CreatedBy.LastName}" : ""))
            .ForMember(d => d.Comments, o => o.MapFrom(s => s.Comments));
        CreateMap<Comment, CommentSummaryDto>()
            .ForMember(d => d.AuthorName, o => o.MapFrom(s => s.Author != null ? $"{s.Author.FirstName} {s.Author.LastName}" : ""));
        CreateMap<CreateTaskItemDto, TaskItem>();
        CreateMap<UpdateTaskItemDto, TaskItem>();

        // Comments
        CreateMap<Comment, CommentDto>()
            .ForMember(d => d.AuthorName, o => o.MapFrom(s => s.Author != null ? $"{s.Author.FirstName} {s.Author.LastName}" : ""));
        CreateMap<CreateCommentDto, Comment>();

        // Users
        CreateMap<ApplicationUser, UserDto>();
    }
}
