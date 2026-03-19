using Microsoft.EntityFrameworkCore;
using TaskFlow.Core.Entities;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Services;
using Xunit;

namespace TaskFlow.Tests.Services;

public class ProjectServiceTests
{
    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task SeedUsersAsync(ApplicationDbContext ctx, params string[] userIds)
    {
        foreach (var id in userIds)
            ctx.Users.Add(new ApplicationUser { Id = id, UserName = id, Email = $"{id}@test.com", NormalizedEmail = $"{id}@TEST.COM", NormalizedUserName = id.ToUpper() });
        await ctx.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAllAsync_Admin_ReturnsAllProjects()
    {
        await using var ctx = CreateContext();
        await SeedUsersAsync(ctx, "user1", "user2");
        ctx.Projects.AddRange(
            new Project { Name = "P1", OwnerId = "user1" },
            new Project { Name = "P2", OwnerId = "user2" }
        );
        await ctx.SaveChangesAsync();

        var sut = new ProjectService(ctx);
        var result = await sut.GetAllAsync("user1", isAdmin: true);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_NonAdmin_ReturnsOnlyOwnedOrMemberProjects()
    {
        await using var ctx = CreateContext();
        await SeedUsersAsync(ctx, "user1", "user2");
        var p1 = new Project { Name = "P1", OwnerId = "user1" };
        var p2 = new Project { Name = "P2", OwnerId = "user2" };
        ctx.Projects.AddRange(p1, p2);
        await ctx.SaveChangesAsync();

        ctx.ProjectMembers.Add(new ProjectMember { ProjectId = p2.Id, UserId = "user1" });
        await ctx.SaveChangesAsync();

        var sut = new ProjectService(ctx);
        var result = await sut.GetAllAsync("user1", isAdmin: false);

        Assert.Equal(2, result.Count()); // owns P1 + member of P2
    }

    [Fact]
    public async Task GetAllAsync_NonAdmin_ExcludesUnrelatedProjects()
    {
        await using var ctx = CreateContext();
        await SeedUsersAsync(ctx, "user1", "user2");
        ctx.Projects.Add(new Project { Name = "Other", OwnerId = "user2" });
        await ctx.SaveChangesAsync();

        var sut = new ProjectService(ctx);
        var result = await sut.GetAllAsync("user1", isAdmin: false);

        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_SetsOwnerAndAddsMemberEntry()
    {
        await using var ctx = CreateContext();
        await SeedUsersAsync(ctx, "owner1");
        var sut = new ProjectService(ctx);

        var project = new Project { Name = "New Project" };
        var created = await sut.CreateAsync(project, "owner1");

        Assert.Equal("owner1", created.OwnerId);
        Assert.True(await ctx.ProjectMembers.AnyAsync(m => m.UserId == "owner1" && m.ProjectId == created.Id));
    }

    [Fact]
    public async Task DeleteAsync_NonAdmin_ThrowsUnauthorized()
    {
        await using var ctx = CreateContext();
        var sut = new ProjectService(ctx);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            sut.DeleteAsync(1, isAdmin: false));
    }
}
