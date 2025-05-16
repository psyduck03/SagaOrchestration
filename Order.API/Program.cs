using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Consumer;
using Order.API.Context;
using Order.API.ViewModels;
using StateMachine.Events.Order;
using StateMachine.Settings;
using Stock.API.Messages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MSSQLServer")));
builder.Services.AddMassTransit(configure =>
{
    configure.AddConsumer<OrderSuccessConsumer>();
    configure.AddConsumer<OrderFailedConsumer>();
    configure.UsingRabbitMq((context, configurator) =>
    {
        configurator.Host(builder.Configuration["RabbitMQ"]);

        configurator.ReceiveEndpoint(RabbitMQSettings.Order_OrderCompletedEventQueue,
            e => e.ConfigureConsumer<OrderSuccessConsumer>(context));
        configurator.ReceiveEndpoint(RabbitMQSettings.Order_OrderFailedEventQueue,
            e => e.ConfigureConsumer<OrderFailedConsumer>(context));
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/create-order",
    async (OrderViewModel model, OrderDbContext context, ISendEndpointProvider sendEndpointProvider) =>
    {
        try
        {
            Order.API.Models.Order order = new()
            {
                BuyerId = model.BuyerId,
                CreatedDate = DateTime.UtcNow,
                OrderStatus = Order.API.Enums.OrderStatus.Suspend,
                TotalPrice = model.OrderItems!.Sum(oi => oi.Count * oi.Price),
                OrderItems = model.OrderItems!.Select(oi => new Order.API.Models.OrderItem
                {
                    Price = oi.Price,
                    Count = oi.Count,
                    ProductId = oi.ProductId,
                }).ToList(),
            };

            await context.Orders.AddAsync(order);
            await context.SaveChangesAsync();

            OrderStartedEvent orderStartedEvent = new()
            {
                BuyerId = model.BuyerId,
                OrderId = order.Id,
                TotalPrice = model.OrderItems!.Sum(oi => oi.Count * oi.Price),
                OrderItems = model.OrderItems!.Select(oi => new OrderItemMessage
                {
                    Price = oi.Price,
                    Count = oi.Count,
                    ProductId = oi.ProductId
                }).ToList()
            };

            ISendEndpoint sendEndpoint =
                await sendEndpointProvider.GetSendEndpoint(new($"queue:{RabbitMQSettings.StateMachine}"));
            await sendEndpoint.Send(orderStartedEvent);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    });


app.UseHttpsRedirection();


app.Run();