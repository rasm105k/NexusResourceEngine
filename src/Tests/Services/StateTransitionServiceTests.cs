using Microsoft.EntityFrameworkCore;
using NexusResourceEngine.Application.DTOs.Transitions;
using NexusResourceEngine.Domain;
using NexusResourceEngine.Infrastructure.Data;
using NexusResourceEngine.Infrastructure.Services;

namespace NexusResourceEngine.Tests.Services;

public class StateTransitionServiceTests
{
    private static StateTransitionService CreateService(string dbName, out Guid tenantId, out Guid stateId)
    {
        var options = new DbContextOptionsBuilder<NexusResourceEngineContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var context = new NexusResourceEngineContext(options);
        tenantId = Guid.NewGuid();
        stateId = Guid.NewGuid();

        context.ResourceStates.Add(new ResourceState
        {
            StateId = stateId,
            TenantId = tenantId,
            Name = "Available",
            ColorCode = "#000",
            SortOrder = 1
        });
        context.SaveChanges();

        return new StateTransitionService(context);
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsDto()
    {
        var service = CreateService(nameof(Create_ValidDto_ReturnsDto), out var tenantId, out var stateId);
        var toStateId = Guid.NewGuid();

        using (var ctx = CreateContext(nameof(Create_ValidDto_ReturnsDto)))
        {
            ctx.ResourceStates.Add(new ResourceState
            {
                StateId = toStateId,
                TenantId = tenantId,
                Name = "Booked",
                ColorCode = "#ff0",
                SortOrder = 2
            });
            ctx.SaveChanges();
        }

        var dto = new CreateStateTransitionDto
        {
            FromStateId = stateId,
            ToStateId = toStateId,
            RequiredRole = "Member"
        };

        var result = await service.CreateAsync(dto, tenantId);

        Assert.NotEqual(Guid.Empty, result.TransitionId);
        Assert.Equal(stateId, result.FromStateId);
        Assert.Equal(toStateId, result.ToStateId);
        Assert.Equal("Member", result.RequiredRole);
    }

    [Fact]
    public async Task Create_FromStateNotFound_ThrowsInvalidOperation()
    {
        var service = CreateService(nameof(Create_FromStateNotFound_ThrowsInvalidOperation), out var tenantId, out _);
        var missingId = Guid.NewGuid();
        var stateId = Guid.NewGuid();

        using (var ctx = CreateContext(nameof(Create_FromStateNotFound_ThrowsInvalidOperation)))
        {
            ctx.ResourceStates.Add(new ResourceState
            {
                StateId = stateId,
                TenantId = tenantId,
                Name = "Booked",
                ColorCode = "#ff0",
                SortOrder = 2
            });
            ctx.SaveChanges();
        }

        var dto = new CreateStateTransitionDto
        {
            FromStateId = missingId,
            ToStateId = stateId,
            RequiredRole = "Admin"
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(dto, tenantId));

        Assert.Contains("FromStateId", ex.Message);
    }

    [Fact]
    public async Task Create_ToStateNotFound_ThrowsInvalidOperation()
    {
        var service = CreateService(nameof(Create_ToStateNotFound_ThrowsInvalidOperation), out var tenantId, out var stateId);

        var dto = new CreateStateTransitionDto
        {
            FromStateId = stateId,
            ToStateId = Guid.NewGuid(),
            RequiredRole = "Admin"
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(dto, tenantId));

        Assert.Contains("ToStateId", ex.Message);
    }

    [Fact]
    public async Task Create_DuplicateTransition_ThrowsInvalidOperation()
    {
        var dbName = nameof(Create_DuplicateTransition_ThrowsInvalidOperation);
        var service = CreateService(dbName, out var tenantId, out var fromStateId);
        var toStateId = Guid.NewGuid();

        using (var ctx = CreateContext(dbName))
        {
            ctx.ResourceStates.Add(new ResourceState
            {
                StateId = toStateId,
                TenantId = tenantId,
                Name = "Booked",
                ColorCode = "#ff0",
                SortOrder = 2
            });
            ctx.SaveChanges();
        }

        var dto = new CreateStateTransitionDto
        {
            FromStateId = fromStateId,
            ToStateId = toStateId,
            RequiredRole = "Member"
        };

        await service.CreateAsync(dto, tenantId);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(dto, tenantId));

        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public async Task Create_EmptyRequiredRole_ThrowsInvalidOperation()
    {
        var service = CreateService(nameof(Create_EmptyRequiredRole_ThrowsInvalidOperation), out var tenantId, out var stateId);
        var toStateId = Guid.NewGuid();

        using (var ctx = CreateContext(nameof(Create_EmptyRequiredRole_ThrowsInvalidOperation)))
        {
            ctx.ResourceStates.Add(new ResourceState
            {
                StateId = toStateId,
                TenantId = tenantId,
                Name = "Booked",
                ColorCode = "#ff0",
                SortOrder = 2
            });
            ctx.SaveChanges();
        }

        var dto = new CreateStateTransitionDto
        {
            FromStateId = stateId,
            ToStateId = toStateId,
            RequiredRole = ""
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(dto, tenantId));

        Assert.Equal("Required role is required.", ex.Message);
    }

    private static NexusResourceEngineContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<NexusResourceEngineContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new NexusResourceEngineContext(options);
    }
}
