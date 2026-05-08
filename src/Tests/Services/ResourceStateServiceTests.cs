using Microsoft.EntityFrameworkCore;
using NexusResourceEngine.Application.DTOs.States;
using NexusResourceEngine.Domain;
using NexusResourceEngine.Infrastructure.Data;
using NexusResourceEngine.Infrastructure.Services;

namespace NexusResourceEngine.Tests.Services;

public class ResourceStateServiceTests
{
    private static ResourceStateService CreateService(string dbName)
    {
        var options = new DbContextOptionsBuilder<NexusResourceEngineContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new ResourceStateService(new NexusResourceEngineContext(options));
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsDtoWithExpectedFields()
    {
        var service = CreateService(nameof(Create_ValidDto_ReturnsDtoWithExpectedFields));
        var tenantId = Guid.NewGuid();

        var dto = new CreateResourceStateDto
        {
            Name = "Available",
            IsBookable = true,
            ColorCode = "#28a745",
            SortOrder = 1
        };

        var result = await service.CreateAsync(dto, tenantId);

        Assert.NotEqual(Guid.Empty, result.StateId);
        Assert.Equal("Available", result.Name);
        Assert.True(result.IsBookable);
        Assert.Equal("#28a745", result.ColorCode);
        Assert.Equal(1, result.SortOrder);
    }

    [Fact]
    public async Task Create_EmptyName_ThrowsInvalidOperation()
    {
        var service = CreateService(nameof(Create_EmptyName_ThrowsInvalidOperation));
        var tenantId = Guid.NewGuid();

        var dto = new CreateResourceStateDto
        {
            Name = "",
            IsBookable = true,
            ColorCode = "#000",
            SortOrder = 1
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(dto, tenantId));

        Assert.Equal("State name is required.", ex.Message);
    }

    [Fact]
    public async Task GetAll_ReturnsStatesForTenant()
    {
        var dbName = nameof(GetAll_ReturnsStatesForTenant);
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        using (var seedCtx = CreateContext(dbName))
        {
            seedCtx.ResourceStates.AddRange(
                new ResourceState { StateId = Guid.NewGuid(), TenantId = tenantA, Name = "State A1", ColorCode = "#000", SortOrder = 1 },
                new ResourceState { StateId = Guid.NewGuid(), TenantId = tenantA, Name = "State A2", ColorCode = "#000", SortOrder = 2 },
                new ResourceState { StateId = Guid.NewGuid(), TenantId = tenantB, Name = "State B1", ColorCode = "#000", SortOrder = 1 }
            );
            seedCtx.SaveChanges();
        }

        var service = CreateService(dbName);
        var result = await service.GetAllAsync(tenantA);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Name == "State A1");
        Assert.Contains(result, r => r.Name == "State A2");
    }

    [Fact]
    public async Task GetAll_ReturnsStatesSortedBySortOrder()
    {
        var dbName = nameof(GetAll_ReturnsStatesSortedBySortOrder);
        var tenantId = Guid.NewGuid();

        using (var seedCtx = CreateContext(dbName))
        {
            seedCtx.ResourceStates.AddRange(
                new ResourceState { StateId = Guid.NewGuid(), TenantId = tenantId, Name = "Z", ColorCode = "#000", SortOrder = 3 },
                new ResourceState { StateId = Guid.NewGuid(), TenantId = tenantId, Name = "A", ColorCode = "#000", SortOrder = 1 },
                new ResourceState { StateId = Guid.NewGuid(), TenantId = tenantId, Name = "M", ColorCode = "#000", SortOrder = 2 }
            );
            seedCtx.SaveChanges();
        }

        var service = CreateService(dbName);
        var result = await service.GetAllAsync(tenantId);

        Assert.Equal(3, result.Count);
        Assert.Equal("A", result[0].Name);
        Assert.Equal("M", result[1].Name);
        Assert.Equal("Z", result[2].Name);
    }

    private static NexusResourceEngineContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<NexusResourceEngineContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new NexusResourceEngineContext(options);
    }
}
