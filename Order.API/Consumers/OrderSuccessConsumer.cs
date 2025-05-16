using MassTransit;
using Order.API.Context;
using Order.API.Enums;
using Order.API.Events;

namespace Order.API.Consumer;

public class OrderSuccessConsumer : IConsumer<OrderSuccessEvent>
{
    private readonly OrderDbContext _orderDbContext;

    public OrderSuccessConsumer(OrderDbContext orderDbContext)
    {
        _orderDbContext = orderDbContext;
    }

    public async Task Consume(ConsumeContext<OrderSuccessEvent> context)
    {
        var order = await _orderDbContext.Orders.FindAsync(context.Message.OrderId);
        if (order != null)
        {
            order.OrderStatus = OrderStatus.Completed;
            await _orderDbContext.SaveChangesAsync();
        }
    }
}