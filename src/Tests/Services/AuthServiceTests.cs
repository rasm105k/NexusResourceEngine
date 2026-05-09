using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NexusResourceEngine.Application.DTOs.Auth;
using NexusResourceEngine.Domain;
using NexusResourceEngine.Infrastructure.Auth;
using NexusResourceEngine.Infrastructure.Data;
using NexusResourceEngine.Infrastructure.Services;

namespace NexusResourceEngine.Tests.Services;

public class AuthServiceTests
{
    private static AuthService CreateService(string dbName)
    {
        var options = new DbContextOptionsBuilder<NexusResourceEngineContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var context = new NexusResourceEngineContext(options);

        var jwtSettings = Options.Create(new JwtSettings
        {
            Key = "TestKeyThatIsAtLeast32CharactersLongForHmac!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryMinutes = 60
        });

        return new AuthService(context, jwtSettings);
    }

    [Fact]
    public async Task Register_CreatesTenantAndUser_ReturnsToken()
    {
        var service = CreateService(nameof(Register_CreatesTenantAndUser_ReturnsToken));

        var request = new RegisterRequestDto
        {
            OrganizationName = "TestOrg",
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!"
        };

        var result = await service.RegisterAsync(request, Guid.NewGuid());

        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.NotEqual(Guid.Empty, result.UserId);
        Assert.Equal("Admin", result.Role);
    }

    [Fact]
    public async Task Register_SavesUserWithHashedPassword()
    {
        var service = CreateService(nameof(Register_SavesUserWithHashedPassword));
        var tenantId = Guid.NewGuid();

        var request = new RegisterRequestDto
        {
            OrganizationName = "TestOrg",
            Username = "hashuser",
            Email = "hash@example.com",
            Password = "Password123!"
        };

        await service.RegisterAsync(request, tenantId);

        var context = CreateContext(nameof(Register_SavesUserWithHashedPassword));
        var user = await context.Users.FirstAsync(u => u.Email == "hash@example.com");
        Assert.NotEqual("Password123!", user.PasswordHash);
        Assert.True(user.PasswordHash.Length > 20);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        var dbName = nameof(Login_ValidCredentials_ReturnsToken);
        var service = CreateService(dbName);
        var tenantId = Guid.NewGuid();

        var register = new RegisterRequestDto
        {
            OrganizationName = "TestOrg",
            Username = "logintest",
            Email = "login@example.com",
            Password = "Password123!"
        };
        await service.RegisterAsync(register, tenantId);

        var login = new LoginRequestDto
        {
            OrganizationName = "TestOrg",
            Email = "login@example.com",
            Password = "Password123!"
        };

        var result = await service.LoginAsync(login);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal("Admin", result.Role);
        Assert.NotEqual(Guid.Empty, result.UserId);
    }

    [Fact]
    public async Task Login_InvalidEmail_ThrowsUnauthorized()
    {
        var dbName = nameof(Login_InvalidEmail_ThrowsUnauthorized);
        var service = CreateService(dbName);
        var tenantId = Guid.NewGuid();

        var register = new RegisterRequestDto
        {
            OrganizationName = "TestOrg",
            Username = "validuser",
            Email = "valid@example.com",
            Password = "Password123!"
        };
        await service.RegisterAsync(register, tenantId);

        var login = new LoginRequestDto
        {
            OrganizationName = "TestOrg",
            Email = "nonexistent@example.com",
            Password = "Password123!"
        };

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(login));

        Assert.Equal("Invalid email or password.", ex.Message);
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsUnauthorized()
    {
        var dbName = nameof(Login_WrongPassword_ThrowsUnauthorized);
        var service = CreateService(dbName);
        var tenantId = Guid.NewGuid();

        var register = new RegisterRequestDto
        {
            OrganizationName = "TestOrg",
            Username = "wrongpw",
            Email = "wrongpw@example.com",
            Password = "CorrectPassword1!"
        };
        await service.RegisterAsync(register, tenantId);

        var login = new LoginRequestDto
        {
            OrganizationName = "TestOrg",
            Email = "wrongpw@example.com",
            Password = "WrongPassword1!"
        };

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(login));

        Assert.Equal("Invalid email or password.", ex.Message);
    }

    [Fact]
    public async Task JwtToken_ContainsExpectedClaims()
    {
        var dbName = nameof(JwtToken_ContainsExpectedClaims);
        var service = CreateService(dbName);
        var tenantId = Guid.NewGuid();

        var request = new RegisterRequestDto
        {
            OrganizationName = "ClaimsTest",
            Username = "claimstest",
            Email = "claims@example.com",
            Password = "Password123!"
        };

        var result = await service.RegisterAsync(request, tenantId);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);

        Assert.Equal(tenantId.ToString(), token.Claims.First(c => c.Type == "TenantId").Value);
        Assert.Equal(result.UserId.ToString(), token.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal("Admin", token.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        Assert.Equal("claims@example.com", token.Claims.First(c => c.Type == ClaimTypes.Email).Value);
    }

    private static NexusResourceEngineContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<NexusResourceEngineContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new NexusResourceEngineContext(options);
    }
}
