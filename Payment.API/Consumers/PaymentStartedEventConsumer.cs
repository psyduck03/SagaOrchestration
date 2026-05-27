using MassTransit;
using Payment.API.Events;
using StateMachine.Events;
using StateMachine.Settings;

namespace Payment.API.Consumers;

public class PaymentStartedEventConsumer : IConsumer<PaymentStartedEvent>
{
    private readonly ISendEndpointProvider _sendEndpointProvider;

    public PaymentStartedEventConsumer(ISendEndpointProvider sendEndpointProvider)
    {
        _sendEndpointProvider = sendEndpointProvider;
    }

    public async Task Consume(ConsumeContext<PaymentStartedEvent> context)
{
    Console.WriteLine($"Payment received. CorrelationId: {context.Message.CorrelationId}, TotalPrice: {context.Message.TotalPrice}");
    var sendEndpoint =
        await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.StateMachine}"));
    if (context.Message.TotalPrice <= 5600)
    {
        Console.WriteLine("Payment approved. Sending PaymentCompletedEvent.");
        await sendEndpoint.Send(new PaymentCompletedEvent(context.Message.CorrelationId));
    }
    else
    {
        Console.WriteLine("Payment declined. Sending PaymentFailedEvent.");
        await sendEndpoint.Send(new PaymentFailedEvent(context.Message.CorrelationId)
        {
            Message = "Insufficient balance!",
            OrderItems = context.Message.OrderItems
        });
    }
}
}
