using Microsoft.Extensions.Configuration;
using TaskFlow.Core.Entities;
using TaskFlow.Infrastructure.Services;
using Xunit;

namespace TaskFlow.Tests.Services;

public class TokenServiceTests
{
    private readonly TokenService _sut;

    public TokenServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "TestSecretKeyThatIsAtLeast32CharactersLong!",
                ["Jwt:Issuer"] = "TaskFlow",
                ["Jwt:Audience"] = "TaskFlow-Users",
                ["Jwt:AccessTokenExpiryMinutes"] = "15",
            })
            .Build();
        _sut = new TokenService(config);
    }

    [Fact]
    public void GenerateJwtToken_ReturnsValidJwtString()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            FirstName = "Jane",
            LastName = "Doe",
            UserName = "test@example.com",
        };

        var token = _sut.GenerateJwtToken(user, new[] { "Member" });

        Assert.NotNull(token);
        Assert.Equal(3, token.Split('.').Length); // JWT has 3 parts
    }

    [Fact]
    public void GenerateJwtToken_ContainsRoleClaim()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "manager@example.com",
            FirstName = "Joe",
            LastName = "Manager",
            UserName = "manager@example.com",
        };

        var token = _sut.GenerateJwtToken(user, new[] { "Manager" });
        var payloadBase64 = token.Split('.')[1];
        var padding = 4 - payloadBase64.Length % 4;
        if (padding < 4) payloadBase64 += new string('=', padding);
        var payload = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payloadBase64));

        Assert.Contains("Manager", payload);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsNonEmptyString()
    {
        var token = _sut.GenerateRefreshToken();
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsDifferentTokensEachTime()
    {
        var token1 = _sut.GenerateRefreshToken();
        var token2 = _sut.GenerateRefreshToken();
        Assert.NotEqual(token1, token2);
    }
}
