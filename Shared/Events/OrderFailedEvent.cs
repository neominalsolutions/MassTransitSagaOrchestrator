using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Events
{

  // Sipariş oluşmadığı durumda Hem Order Hemde Stock Servise bildirim gönderir.
  public class OrderFailedEvent
  {
    public int OrderId { get; set; }
    public string Message { get; set; }
  }
}
