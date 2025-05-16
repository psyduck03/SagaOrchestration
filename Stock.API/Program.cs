using MassTransit;
using MongoDB.Driver;
using StateMachine.Settings;
using Stock.API.Consumers;
using Stock.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<MongoDbService>();

builder.Services.AddMassTransit(configure =>
{
    configure.AddConsumer<OrderCreatedEventConsumer>();
    configure.AddConsumer<StockRollbackMessageConsumer>();
    configure.UsingRabbitMq((context, configurator) =>
    {
        configurator.Host(builder.Configuration["RabbitMQ"]);
        configurator.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventQueue,
            e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
        configurator.ReceiveEndpoint(RabbitMQSettings.Stock_RollbackMessageQueue,
            e => e.ConfigureConsumer<StockRollbackMessageConsumer>(context));
    });
});

using var scope = builder.Services.BuildServiceProvider().CreateScope();
var mongoDbService = scope.ServiceProvider.GetRequiredService<MongoDbService>();

if (!await (await mongoDbService.GetCollection<Stock.API.Models.Stock>().FindAsync(x => true)).AnyAsync())
{
    mongoDbService.GetCollection<Stock.API.Models.Stock>().InsertOne(new()
    {
        ProductId = 1,
        Count = 200
    });
    mongoDbService.GetCollection<Stock.API.Models.Stock>().InsertOne(new()
    {
        ProductId = 2,
        Count = 300
    });
    mongoDbService.GetCollection<Stock.API.Models.Stock>().InsertOne(new()
    {
        ProductId = 3,
        Count = 50
    });
    mongoDbService.GetCollection<Stock.API.Models.Stock>().InsertOne(new()
    {
        ProductId = 4,
        Count = 10
    });
    mongoDbService.GetCollection<Stock.API.Models.Stock>().InsertOne(new()
    {
        ProductId = 5,
        Count = 60
    });
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.Run();