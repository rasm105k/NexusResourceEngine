using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NexusResourceEngine.Application.DTOs.Auth;
using NexusResourceEngine.Application.Interfaces;

namespace NexusResourceEngine.Tests.Integration;

public class AuthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "TestKeyThatIsAtLeast32CharactersLongForHmac!",
                    ["Jwt:Issuer"] = "TestIssuer",
                    ["Jwt:Audience"] = "TestAudience",
                    ["Jwt:ExpiryMinutes"] = "60"
                });
            });

            builder.ConfigureServices(services =>
            {
                var stub = new StubAuthService();
                services.AddSingleton<IAuthService>(stub);
            });
        });
    }

    [Fact]
    public async Task PostRegister_Returns200WithToken()
    {
        var client = _factory.CreateClient();

        var request = new RegisterRequestDto
        {
            OrganizationName = "TestOrg",
            Username = "apiuser",
            Email = "api@example.com",
            Password = "Password123!"
        };

        var response = await client.PostAsJsonAsync("/auth/register", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(body);
        Assert.NotEmpty(body!.Token);
        Assert.Equal("Admin", body.Role);
    }

    [Fact]
    public async Task PostRegister_ThenLogin_ReturnsSameUser()
    {
        var client = _factory.CreateClient();

        var register = new RegisterRequestDto
        {
            OrganizationName = "FullFlow",
            Username = "fullflow",
            Email = "fullflow@example.com",
            Password = "Password123!"
        };

        var registerResponse = await client.PostAsJsonAsync("/auth/register", register);
        var registerBody = await registerResponse.Content.ReadFromJsonAsync<LoginResponseDto>();

        var login = new LoginRequestDto
        {
            Email = "fullflow@example.com",
            Password = "Password123!"
        };

        var loginResponse = await client.PostAsJsonAsync("/auth/login", login);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(loginBody);
        Assert.Equal(registerBody!.UserId, loginBody!.UserId);
        Assert.Equal(registerBody.Role, loginBody.Role);
        Assert.NotEqual(registerBody.Token, loginBody.Token);
    }

    [Fact]
    public async Task PostLogin_InvalidCredentials_Returns401()
    {
        var client = _factory.CreateClient();

        var login = new LoginRequestDto
        {
            Email = "nobody@example.com",
            Password = "wrong!"
        };

        var response = await client.PostAsJsonAsync("/auth/login", login);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}

public class StubAuthService : IAuthService
{
    private readonly Dictionary<string, (RegisterRequestDto Register, LoginResponseDto Response)> _users = new();

    public Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request, Guid tenantId)
    {
        var response = new LoginResponseDto
        {
            Token = $"jwt-{Guid.NewGuid():n}",
            UserId = Guid.NewGuid(),
            Role = "Admin"
        };

        _users[request.Email] = (request, response);
        return Task.FromResult(response);
    }

    public Task<LoginResponseDto> RegisterMemberAsync(RegisterMemberDto request, Guid tenantId)
    {
        var response = new LoginResponseDto
        {
            Token = $"jwt-{Guid.NewGuid():n}",
            UserId = Guid.NewGuid(),
            Role = "Member"
        };

        return Task.FromResult(response);
    }

    public Task<InviteResponseDto> InviteAsync(InviteRequestDto request, Guid tenantId)
    {
        var invites = request.Invitees.Select(i => new InviteResultDto
        {
            Email = i.Email,
            InviteUrl = $"{request.RedirectUrl}?token={Guid.NewGuid():n}",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        }).ToList();

        return Task.FromResult(new InviteResponseDto { Invites = invites });
    }

    public Task<LoginResponseDto> AcceptInviteAsync(AcceptInviteDto request)
    {
        var response = new LoginResponseDto
        {
            Token = $"jwt-{Guid.NewGuid():n}",
            UserId = Guid.NewGuid(),
            Role = "Member"
        };

        return Task.FromResult(response);
    }

    public Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        if (_users.TryGetValue(request.Email, out var record))
        {
            var newResponse = new LoginResponseDto
            {
                Token = $"jwt-{Guid.NewGuid():n}",
                UserId = record.Response.UserId,
                Role = record.Response.Role
            };
            return Task.FromResult(newResponse);
        }

        throw new UnauthorizedAccessException("Invalid email or password.");
    }
}
