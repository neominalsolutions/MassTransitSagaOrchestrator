using MassTransit;
using Shared;
using Shared.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payment.API.Consumers
{
    public class StartPaymentCommandConsumer : IConsumer<StartPaymentCommand>
    {
        readonly ISendEndpointProvider _sendEndpointProvider;
       
        public StartPaymentCommandConsumer(ISendEndpointProvider sendEndpointProvider)
        {
            this._sendEndpointProvider = sendEndpointProvider;
        }

        public async Task Consume(ConsumeContext<StartPaymentCommand> context)
        {
            ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.StateMachine}"));
            if (context.Message.TotalPrice <= 100)
                await sendEndpoint.Send(new PaymentCompletedCommand(context.Message.CorrelationId));
            else
                await sendEndpoint.Send(new PaymentFailedCommand(context.Message.CorrelationId)
                {
                    Message = "Bakiye yetersiz!",
                    OrderId = context.Message.OrderId
                });
        }
    }
}
