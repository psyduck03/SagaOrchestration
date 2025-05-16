using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using StateMachine.StateMaps;

namespace StateMachine.StateDbContexts;

public class OrderStateDbContext : SagaDbContext
{
    public OrderStateDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override IEnumerable<ISagaClassMap> Configurations
    {
        get { yield return new OrderStateMap(); }
    }
}