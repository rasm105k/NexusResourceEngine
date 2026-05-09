using Microsoft.EntityFrameworkCore;
using NexusResourceEngine.Application.DTOs.Resources;
using NexusResourceEngine.Domain;
using NexusResourceEngine.Infrastructure.Data;
using NexusResourceEngine.Infrastructure.Services;

namespace NexusResourceEngine.Tests.Services;

public class ResourceServiceTests
{
    private static ResourceService CreateService(string dbName)
    {
        var options = new DbContextOptionsBuilder<NexusResourceEngineContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new ResourceService(new NexusResourceEngineContext(options));
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsDtoWithExpectedFields()
    {
        var dbName = nameof(Create_ValidDto_ReturnsDtoWithExpectedFields);
        var tenantId = Guid.NewGuid();
        var stateId = Guid.NewGuid();

        using (var ctx = CreateContext(dbName))
        {
            ctx.ResourceStates.Add(new ResourceState
            {
                StateId = stateId,
                TenantId = tenantId,
                Name = "Available",
                IsBookable = true,
                ColorCode = "#28a745",
                SortOrder = 1
            });
            ctx.SaveChanges();
        }

        var service = CreateService(dbName);
        var dto = new CreateResourceDto
        {
            Name = "Conference Room A",
            Description = "Ground floor",
            CurrentStateId = stateId,
            Latitude = 51.5074m,
            Longitude = -0.1278m
        };

        var result = await service.CreateAsync(dto, tenantId);

        Assert.NotEqual(Guid.Empty, result.ResourceId);
        Assert.Equal("Conference Room A", result.Name);
        Assert.Equal(stateId, result.CurrentStateId);
        Assert.Equal(51.5074m, result.Latitude);
        Assert.Equal(-0.1278m, result.Longitude);
    }

    [Fact]
    public async Task Create_EmptyName_ThrowsInvalidOperation()
    {
        var service = CreateService(nameof(Create_EmptyName_ThrowsInvalidOperation));
        var dto = new CreateResourceDto
        {
            Name = "",
            CurrentStateId = Guid.NewGuid()
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(dto, Guid.NewGuid()));

        Assert.Equal("Resource name is required.", ex.Message);
    }

    [Fact]
    public async Task Create_MissingStateId_ThrowsInvalidOperation()
    {
        var service = CreateService(nameof(Create_MissingStateId_ThrowsInvalidOperation));
        var dto = new CreateResourceDto
        {
            Name = "Test Resource"
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(dto, Guid.NewGuid()));

        Assert.Equal("CurrentStateId is required.", ex.Message);
    }

    [Fact]
    public async Task Create_InvalidStateId_ThrowsInvalidOperation()
    {
        var dbName = nameof(Create_InvalidStateId_ThrowsInvalidOperation);
        var tenantId = Guid.NewGuid();

        var service = CreateService(dbName);
        var dto = new CreateResourceDto
        {
            Name = "Test Resource",
            CurrentStateId = Guid.NewGuid()
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(dto, tenantId));

        Assert.Equal("CurrentStateId does not reference an existing state for this tenant.", ex.Message);
    }

    [Fact]
    public async Task GetAll_ReturnsResourcesForTenant()
    {
        var dbName = nameof(GetAll_ReturnsResourcesForTenant);
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var stateId = Guid.NewGuid();

        using (var ctx = CreateContext(dbName))
        {
            ctx.ResourceStates.Add(new ResourceState { StateId = stateId, TenantId = tenantA, Name = "Available", IsBookable = true, ColorCode = "#000", SortOrder = 1 });
            ctx.Resources.AddRange(
                new Resource { ResourceId = Guid.NewGuid(), TenantId = tenantA, Name = "Resource A1", CurrentStateId = stateId },
                new Resource { ResourceId = Guid.NewGuid(), TenantId = tenantA, Name = "Resource A2", CurrentStateId = stateId },
                new Resource { ResourceId = Guid.NewGuid(), TenantId = tenantB, Name = "Resource B1", CurrentStateId = stateId }
            );
            ctx.SaveChanges();
        }

        var service = CreateService(dbName);
        var result = await service.GetAllAsync(tenantA);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Name == "Resource A1");
        Assert.Contains(result, r => r.Name == "Resource A2");
    }

    [Fact]
    public async Task GetAll_FiltersByStateId()
    {
        var dbName = nameof(GetAll_FiltersByStateId);
        var tenantId = Guid.NewGuid();
        var stateA = Guid.NewGuid();
        var stateB = Guid.NewGuid();

        using (var ctx = CreateContext(dbName))
        {
            ctx.ResourceStates.AddRange(
                new ResourceState { StateId = stateA, TenantId = tenantId, Name = "Available", IsBookable = true, ColorCode = "#000", SortOrder = 1 },
                new ResourceState { StateId = stateB, TenantId = tenantId, Name = "Maintenance", IsBookable = false, ColorCode = "#000", SortOrder = 2 }
            );
            ctx.Resources.AddRange(
                new Resource { ResourceId = Guid.NewGuid(), TenantId = tenantId, Name = "Room 1", CurrentStateId = stateA },
                new Resource { ResourceId = Guid.NewGuid(), TenantId = tenantId, Name = "Room 2", CurrentStateId = stateB },
                new Resource { ResourceId = Guid.NewGuid(), TenantId = tenantId, Name = "Room 3", CurrentStateId = stateA }
            );
            ctx.SaveChanges();
        }

        var service = CreateService(dbName);
        var result = await service.GetAllAsync(tenantId, stateA);

        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(stateA, r.CurrentStateId));
    }

    [Fact]
    public async Task ChangeState_ValidTransition_UpdatesState()
    {
        var dbName = nameof(ChangeState_ValidTransition_UpdatesState);
        var tenantId = Guid.NewGuid();
        var fromState = Guid.NewGuid();
        var toState = Guid.NewGuid();
        var resourceId = Guid.NewGuid();

        using (var ctx = CreateContext(dbName))
        {
            ctx.ResourceStates.AddRange(
                new ResourceState { StateId = fromState, TenantId = tenantId, Name = "Available", IsBookable = true, ColorCode = "#000", SortOrder = 1 },
                new ResourceState { StateId = toState, TenantId = tenantId, Name = "Maintenance", IsBookable = false, ColorCode = "#000", SortOrder = 2 }
            );
            ctx.StateTransitions.Add(new StateTransition
            {
                TransitionId = Guid.NewGuid(),
                TenantId = tenantId,
                FromStateId = fromState,
                ToStateId = toState,
                RequiredRole = "Admin"
            });
            ctx.Resources.Add(new Resource
            {
                ResourceId = resourceId,
                TenantId = tenantId,
                Name = "Test Resource",
                CurrentStateId = fromState
            });
            ctx.SaveChanges();
        }

        var service = CreateService(dbName);
        var dto = new UpdateResourceStateDto { NewStateId = toState };
        var result = await service.ChangeStateAsync(resourceId, dto, tenantId, "Admin");

        Assert.Equal(toState, result.CurrentStateId);
    }

    [Fact]
    public async Task ChangeState_NoTransition_ThrowsInvalidOperation()
    {
        var dbName = nameof(ChangeState_NoTransition_ThrowsInvalidOperation);
        var tenantId = Guid.NewGuid();
        var fromState = Guid.NewGuid();
        var toState = Guid.NewGuid();
        var resourceId = Guid.NewGuid();

        using (var ctx = CreateContext(dbName))
        {
            ctx.ResourceStates.AddRange(
                new ResourceState { StateId = fromState, TenantId = tenantId, Name = "Available", IsBookable = true, ColorCode = "#000", SortOrder = 1 },
                new ResourceState { StateId = toState, TenantId = tenantId, Name = "Maintenance", IsBookable = false, ColorCode = "#000", SortOrder = 2 }
            );
            ctx.Resources.Add(new Resource
            {
                ResourceId = resourceId,
                TenantId = tenantId,
                Name = "Test Resource",
                CurrentStateId = fromState
            });
            ctx.SaveChanges();
        }

        var service = CreateService(dbName);
        var dto = new UpdateResourceStateDto { NewStateId = toState };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ChangeStateAsync(resourceId, dto, tenantId, "Admin"));

        Assert.Equal("No valid transition exists between the current state and the target state.", ex.Message);
    }

    [Fact]
    public async Task ChangeState_WrongRole_ThrowsUnauthorized()
    {
        var dbName = nameof(ChangeState_WrongRole_ThrowsUnauthorized);
        var tenantId = Guid.NewGuid();
        var fromState = Guid.NewGuid();
        var toState = Guid.NewGuid();
        var resourceId = Guid.NewGuid();

        using (var ctx = CreateContext(dbName))
        {
            ctx.ResourceStates.AddRange(
                new ResourceState { StateId = fromState, TenantId = tenantId, Name = "Available", IsBookable = true, ColorCode = "#000", SortOrder = 1 },
                new ResourceState { StateId = toState, TenantId = tenantId, Name = "Maintenance", IsBookable = false, ColorCode = "#000", SortOrder = 2 }
            );
            ctx.StateTransitions.Add(new StateTransition
            {
                TransitionId = Guid.NewGuid(),
                TenantId = tenantId,
                FromStateId = fromState,
                ToStateId = toState,
                RequiredRole = "Admin"
            });
            ctx.Resources.Add(new Resource
            {
                ResourceId = resourceId,
                TenantId = tenantId,
                Name = "Test Resource",
                CurrentStateId = fromState
            });
            ctx.SaveChanges();
        }

        var service = CreateService(dbName);
        var dto = new UpdateResourceStateDto { NewStateId = toState };

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.ChangeStateAsync(resourceId, dto, tenantId, "Member"));

        Assert.Equal("Your role does not have permission to perform this state transition.", ex.Message);
    }

    [Fact]
    public async Task ChangeState_ResourceNotFound_ThrowsInvalidOperation()
    {
        var service = CreateService(nameof(ChangeState_ResourceNotFound_ThrowsInvalidOperation));
        var dto = new UpdateResourceStateDto { NewStateId = Guid.NewGuid() };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ChangeStateAsync(Guid.NewGuid(), dto, Guid.NewGuid(), "Admin"));

        Assert.Equal("Resource not found.", ex.Message);
    }

    private static NexusResourceEngineContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<NexusResourceEngineContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new NexusResourceEngineContext(options);
    }
}
