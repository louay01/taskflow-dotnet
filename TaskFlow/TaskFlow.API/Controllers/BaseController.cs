using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected string GetCurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("User ID not found in token.");

    protected bool IsAdmin() => User.IsInRole("Admin");
    protected bool IsManager() => User.IsInRole("Manager") || User.IsInRole("Admin");
}
