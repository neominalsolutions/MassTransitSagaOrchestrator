using MassTransit;
using Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Commands
{
     // Siparişteki ürünler rezerve edildiği durumda State Machine Servise gönderilir.
    public class StockReservedCommand : CorrelatedBy<Guid>
    {
        public StockReservedCommand(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public Guid CorrelationId { get; }

        public int OrderId { get; set; }
    }
}
