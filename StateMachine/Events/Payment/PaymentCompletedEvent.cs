using MassTransit;

namespace Payment.API.Events;

public class PaymentCompletedEvent : CorrelatedBy<Guid>
{
    public PaymentCompletedEvent(Guid correlationId)
    {
        CorrelationId = correlationId;
    }

    public Guid CorrelationId { get; }
}