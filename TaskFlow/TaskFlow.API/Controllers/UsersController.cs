using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.API.DTOs.Users;
using TaskFlow.Core.Entities;

namespace TaskFlow.API.Controllers;

/// <summary>User management — profile, admin operations.</summary>
[Authorize]
public class UsersController : BaseController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public UsersController(UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    /// <summary>Get all users. Admin only.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userManager.Users.ToListAsync();
        var dtos = new List<UserDto>();
        foreach (var user in users)
        {
            var dto = _mapper.Map<UserDto>(user);
            dto.Roles = await _userManager.GetRolesAsync(user);
            dtos.Add(dto);
        }
        return Ok(dtos);
    }

    /// <summary>Get a user by ID. Admin or the user themselves.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        if (!IsAdmin() && GetCurrentUserId() != id)
            return Forbid();

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var dto = _mapper.Map<UserDto>(user);
        dto.Roles = await _userManager.GetRolesAsync(user);
        return Ok(dto);
    }

    /// <summary>Update a user's profile. Admin or the user themselves.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto dto)
    {
        if (!IsAdmin() && GetCurrentUserId() != id)
            return Forbid();

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        await _userManager.UpdateAsync(user);

        var result = _mapper.Map<UserDto>(user);
        result.Roles = await _userManager.GetRolesAsync(user);
        return Ok(result);
    }

    /// <summary>Deactivate a user. Admin only.</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Deactivate(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        user.IsActive = false;
        await _userManager.UpdateAsync(user);
        return NoContent();
    }

    /// <summary>Promote a user to Manager. Admin only.</summary>
    [HttpPost("{id}/promote")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Promote(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        if (!await _userManager.IsInRoleAsync(user, "Manager"))
            await _userManager.AddToRoleAsync(user, "Manager");

        return NoContent();
    }

    /// <summary>Demote a Manager back to Member. Admin only.</summary>
    [HttpPost("{id}/demote")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Demote(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        if (await _userManager.IsInRoleAsync(user, "Manager"))
            await _userManager.RemoveFromRoleAsync(user, "Manager");

        return NoContent();
    }
}
