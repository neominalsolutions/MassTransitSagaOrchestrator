using MassTransit;
using Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Commands
{
     // SubmitOrder işleminden sonra, yani siparişler kaydı alındıktan sonra Siparişe ait Stokların Kontrol edilmesi için kullanılır.
     // State Machine Service üzerinden Stock API gönderilerek süreç başlatılır. 
    public class CheckStockCommand : CorrelatedBy<Guid>
    {
        public CheckStockCommand(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public Guid CorrelationId { get; }

        public int OrderId { get; set; }
        public List<OrderItemMessage> OrderItems { get; set; }
    }
}
