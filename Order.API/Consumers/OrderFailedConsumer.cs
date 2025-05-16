using MassTransit;
using Order.API.Context;
using Order.API.Enums;
using Order.API.Events;

namespace Order.API.Consumer;

public class OrderFailedConsumer : IConsumer<OrderFailedEvent>
{
    private readonly OrderDbContext _orderDbContext;

    public OrderFailedConsumer(OrderDbContext orderDbContext)
    {
        _orderDbContext = orderDbContext;
    }

    public async Task Consume(ConsumeContext<OrderFailedEvent> context)
    {
        var order = await _orderDbContext.FindAsync<Models.Order>(context.Message.OrderId);
        if (order != null)
        {
            order.OrderStatus = OrderStatus.Fail;
            await _orderDbContext.SaveChangesAsync();
        }
    }
}