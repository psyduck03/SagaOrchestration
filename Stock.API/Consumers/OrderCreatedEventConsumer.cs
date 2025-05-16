using MassTransit;
using MongoDB.Driver;
using StateMachine.Settings;
using Stock.API.Events;
using Stock.API.Messages;
using Stock.API.Services;

namespace Stock.API.Consumers;

public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly MongoDbService _mongoDbService;
    private readonly ISendEndpointProvider _sendEndpointProvider;

    public OrderCreatedEventConsumer(
        MongoDbService mongoDbService,
        ISendEndpointProvider sendEndpointProvider)
    {
        _mongoDbService = mongoDbService;
        _sendEndpointProvider = sendEndpointProvider;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var sendEndpoint =
            await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.StateMachine}"));
        List<bool> stockResult = new();
        var collection = _mongoDbService.GetCollection<Models.Stock>();

        //Stock control
        foreach (OrderItemMessage orderItem in context.Message.OrderItems!)
            stockResult.Add(
                (await collection.FindAsync(s => s.ProductId == orderItem.ProductId && s.Count > orderItem.Count))
                .Any());

        //StockUpdated.
        if (stockResult.TrueForAll(sr => sr.Equals(true)))
        {
            foreach (OrderItemMessage orderItem in context.Message.OrderItems)
            {
                var stock = await (await collection.FindAsync(s => s.ProductId == orderItem.ProductId))
                    .FirstOrDefaultAsync();
                stock.Count -= orderItem.Count;
                await collection.FindOneAndReplaceAsync(x => x.ProductId == orderItem.ProductId, stock);
            }

            StockReservedEvent stockReservedEvent = new(context.Message.CorrelationId)
            {
                OrderItems = context.Message.OrderItems
            };
            await sendEndpoint.Send(stockReservedEvent);
        }
        //StockFailed.
        else
        {
            var stockNotReservedEvent = new StockNotReservedEvent(context.Message.CorrelationId)
            {
                Message = "Insufficient stock."
            };

            await sendEndpoint.Send(stockNotReservedEvent);
        }
    }
}