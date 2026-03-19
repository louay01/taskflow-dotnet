using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.API.DTOs.Auth;
using TaskFlow.Core.Interfaces;

namespace TaskFlow.API.Controllers;

/// <summary>Authentication endpoints — register, login, refresh, revoke.</summary>
public class AuthController : BaseController
{
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;

    public AuthController(IAuthService authService, IMapper mapper)
    {
        _authService = authService;
        _mapper = mapper;
    }

    /// <summary>Register a new user. Automatically assigned the Member role.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(
            new RegisterRequest(dto.FirstName, dto.LastName, dto.Email, dto.Password));
        return CreatedAtAction(nameof(Register), _mapper.Map<AuthResponseDto>(result));
    }

    /// <summary>Login and receive a JWT access token and refresh token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(new LoginRequest(dto.Email, dto.Password));
        return Ok(_mapper.Map<AuthResponseDto>(result));
    }

    /// <summary>Exchange a refresh token for a new JWT + refresh token pair.</summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
    {
        var result = await _authService.RefreshAsync(dto.RefreshToken);
        return Ok(_mapper.Map<AuthResponseDto>(result));
    }

    /// <summary>Revoke a refresh token (logout).</summary>
    [HttpPost("revoke")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Revoke([FromBody] RefreshTokenDto dto)
    {
        await _authService.RevokeAsync(dto.RefreshToken);
        return NoContent();
    }
}
