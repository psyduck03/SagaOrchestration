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
    Console.WriteLine($"OrderItems count: {context.Message.OrderItems?.Count}");
    var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.StateMachine}"));
    Console.WriteLine("Send endpoint retrieved.");
    List<bool> stockResult = new();
    var collection = _mongoDbService.GetCollection<Models.Stock>();
    Console.WriteLine("Collection retrieved.");
    foreach (OrderItemMessage orderItem in context.Message.OrderItems!)
    {
        var result = (await collection.FindAsync(s => s.ProductId == orderItem.ProductId && s.Count > orderItem.Count)).Any();
        Console.WriteLine($"Stock check for ProductId {orderItem.ProductId}: {result}");
        stockResult.Add(result);
    }
    Console.WriteLine($"Stock validation complete. Sufficient: {stockResult.TrueForAll(sr => sr.Equals(true))}");
    if (stockResult.TrueForAll(sr => sr.Equals(true)))
    {
        foreach (OrderItemMessage orderItem in context.Message.OrderItems)
        {
            var stock = await (await collection.FindAsync(s => s.ProductId == orderItem.ProductId)).FirstOrDefaultAsync();
            stock.Count -= orderItem.Count;
            await collection.FindOneAndReplaceAsync(x => x.ProductId == orderItem.ProductId, stock);
        }
        Console.WriteLine("Stock updated. Sending StockReservedEvent.");
        StockReservedEvent stockReservedEvent = new(context.Message.CorrelationId)
        {
            OrderItems = context.Message.OrderItems
        };
        await sendEndpoint.Send(stockReservedEvent);
    }
    else
    {
        Console.WriteLine("Insufficient stock. Sending StockNotReservedEvent.");
        var stockNotReservedEvent = new StockNotReservedEvent(context.Message.CorrelationId)
        {
            Message = "Insufficient stock."
        };
        await sendEndpoint.Send(stockNotReservedEvent);
    }
}
}
