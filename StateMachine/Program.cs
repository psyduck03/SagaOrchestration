using MassTransit;
using Microsoft.EntityFrameworkCore;
using StateMachine.Settings;
using StateMachine.StateInstances;
using StateMachine.StateDbContexts;
using StateMachine.StateMachines;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>()
        .EntityFrameworkRepository(options =>
        {
            options.AddDbContext<DbContext, OrderStateDbContext>((provider, _builder) =>
            {
                _builder.UseSqlServer(builder.Configuration.GetConnectionString("MSSQLServer"));
            });
        });

    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);
        _configure.ReceiveEndpoint(RabbitMQSettings.StateMachine, e =>
            e.ConfigureSaga<OrderStateInstance>(context));
    });
});

var app = builder.Build();
app.Run();