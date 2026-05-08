using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NexusResourceEngine.Application.DTOs.States;
using NexusResourceEngine.Application.DTOs.Transitions;
using NexusResourceEngine.Application.Interfaces;

namespace NexusResourceEngine.Tests.Integration;

public class StateEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public StateEndpointTests(WebApplicationFactory<Program> factory)
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
                services.AddSingleton<IResourceStateService>(new StubResourceStateService());
                services.AddSingleton<IStateTransitionService>(new StubStateTransitionService());

                services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = "TestIssuer",
                        ValidateAudience = true,
                        ValidAudience = "TestAudience",
                        ValidateLifetime = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes("TestKeyThatIsAtLeast32CharactersLongForHmac!"))
                    };
                });
            });
        });
    }

    [Fact]
    public async Task PostStates_Returns201WithStateDto()
    {
        var client = _factory.CreateClient();
        var token = CreateTestToken();

        var request = new HttpRequestMessage(HttpMethod.Post, "/states")
        {
            Content = JsonContent.Create(new CreateResourceStateDto
            {
                Name = "Available",
                IsBookable = true,
                ColorCode = "#28a745",
                SortOrder = 1
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.SendAsync(request);

        if (response.StatusCode != HttpStatusCode.Created)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Expected 201 but got {(int)response.StatusCode}: {error}");
        }

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ResourceStateDto>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body!.StateId);
        Assert.Equal("Available", body.Name);
    }

    [Fact]
    public async Task PostTransitions_Returns201WithTransitionDto()
    {
        var client = _factory.CreateClient();
        var token = CreateTestToken();

        var request = new HttpRequestMessage(HttpMethod.Post, "/transitions")
        {
            Content = JsonContent.Create(new CreateStateTransitionDto
            {
                FromStateId = Guid.NewGuid(),
                ToStateId = Guid.NewGuid(),
                RequiredRole = "Admin"
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.SendAsync(request);

        if (response.StatusCode != HttpStatusCode.Created)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Expected 201 but got {(int)response.StatusCode}: {error}");
        }

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<StateTransitionDto>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body!.TransitionId);
        Assert.Equal("Admin", body.RequiredRole);
    }

    private static string CreateTestToken()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TestKeyThatIsAtLeast32CharactersLongForHmac!"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("TenantId", Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var token = new JwtSecurityToken(
            issuer: "TestIssuer",
            audience: "TestAudience",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class StubResourceStateService : IResourceStateService
{
    public Task<List<ResourceStateDto>> GetAllAsync(Guid tenantId)
        => Task.FromResult(new List<ResourceStateDto>());

    public Task<ResourceStateDto> CreateAsync(CreateResourceStateDto dto, Guid tenantId)
    {
        return Task.FromResult(new ResourceStateDto
        {
            StateId = Guid.NewGuid(),
            Name = dto.Name,
            IsBookable = dto.IsBookable,
            ColorCode = dto.ColorCode,
            SortOrder = dto.SortOrder
        });
    }
}

public class StubStateTransitionService : IStateTransitionService
{
    public Task<StateTransitionDto> CreateAsync(CreateStateTransitionDto dto, Guid tenantId)
    {
        return Task.FromResult(new StateTransitionDto
        {
            TransitionId = Guid.NewGuid(),
            FromStateId = dto.FromStateId,
            ToStateId = dto.ToStateId,
            RequiredRole = dto.RequiredRole
        });
    }
}
