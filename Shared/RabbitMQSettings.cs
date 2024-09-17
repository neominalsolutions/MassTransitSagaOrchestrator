using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public static class RabbitMQSettings
    {
        public const string StateMachine = "state-machine-queue";
        public const string CheckStockQuee = "stock-order-created-queue";
        public const string Payment_StartedEventQueue = "payment-started-queue";
        public const string OrderCompletedEventQueue = "order-completed-queue";
        public const string OrderFailedEventQueue = "order-failed-queue";
        public const string StockRollbackMessageQueue = "stock-roolback-queue";
    }
}
