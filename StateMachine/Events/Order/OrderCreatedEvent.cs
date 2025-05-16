using MassTransit;
using Stock.API.Messages;

namespace Stock.API.Events;

public class OrderCreatedEvent : CorrelatedBy<Guid>
{
    public OrderCreatedEvent(Guid correlationId)
    {
        CorrelationId = correlationId;
    }

    public Guid CorrelationId { get; }
    public List<OrderItemMessage>? OrderItems { get; set; }
}