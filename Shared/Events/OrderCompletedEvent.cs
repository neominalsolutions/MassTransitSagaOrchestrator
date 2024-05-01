using Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Events
{
  // Sipariş oluştuğu durumda Hem Order Hemde Stock Servise bildirim gönderir.Stock Servis Reserve edilen Stokları, stoktan düşer, Order Servis ödeme alındığı için siparişi Onaylandı State'ine çeker.
  public class OrderCompletedEvent
    {
        public int OrderId { get; set; }

        public List<OrderItemMessage> OrderItems { get; set; }
  }
}
