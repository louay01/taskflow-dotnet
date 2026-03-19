using System.ComponentModel.DataAnnotations;

namespace TaskFlow.API.DTOs.Auth;

public class RefreshTokenDto
{
    [Required] public string RefreshToken { get; set; } = string.Empty;
}
