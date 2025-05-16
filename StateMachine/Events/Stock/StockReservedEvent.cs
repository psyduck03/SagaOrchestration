using MassTransit;
using Stock.API.Messages;

namespace Stock.API.Events;

public class StockReservedEvent : CorrelatedBy<Guid>
{
    public StockReservedEvent(Guid correlationId)
    {
        CorrelationId = correlationId;
    }

    public Guid CorrelationId { get; }
    public List<OrderItemMessage>? OrderItems { get; set; }
}