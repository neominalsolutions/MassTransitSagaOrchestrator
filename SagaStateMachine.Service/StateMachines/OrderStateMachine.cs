using Automatonymous;
using SagaStateMachine.Service.Instruments;
using Shared;
using Shared.Commands;
using Shared.Events;
using Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SagaStateMachine.Service.StateMachines
{
    public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
    {
        public Event<SubmitOrderCommand> OrderSubmitted { get; set; }
        public Event<StockReservedCommand> StockReserved { get; set; }
        public Event<PaymentCompletedCommand> PaymentCompleted { get; set; }
        public Event<PaymentFailedCommand> PaymentFailed { get; set; }
        public Event<StockNotreservedCommand> StockNotReserved { get; set; }

    // Önemli: State Machine içerisinde sadece State Machine tarafından dinlenecek olan eventler tanımlanır. Dinleyici eventler durum değişikliklerinin yakalanması ve sürecin yönetilmesi için kullanılır.
    // Bu eventler süreci API devremek için farklı Eventleri Queue devredebilir. 

        public State OrderSubmmitedState { get; set; }
        public State StockReservedState { get; set; }
        public State PaymentCompletedState { get; set; }
        public State PaymentFailedState { get; set; }
        public State StockNotReservedState { get; set; }
        public OrderStateMachine()
        {
            //State Instance'da ki hangi property'nin sipariş sürecindeki state'i tutacağı bildiriliyor.
            //Yani artık tüm event'ler CurrentState property'sin de tutulacaktır!
            InstanceState(instance => instance.CurrentState);

      // Süreci Order Started Eventinden başladığımız için bir Corrlation yani Süreç takip Idsi ürettik. Diğer Eventler aynı Id üzerinden süreçlerini devam ettirecektir. aşığdaki  orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId)); kodu bunun için kullanılmıştır.

            Event(() => OrderSubmitted,
                orderStateInstance =>
                orderStateInstance.CorrelateBy<int>(database => database.OrderId, @event => @event.Message.OrderId)
                .SelectId(e => Guid.NewGuid()));

            Event(() => StockReserved,
                orderStateInstance =>
                orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

            Event(() => StockNotReserved,
                orderStateInstance =>
                orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));
    
            Event(() => PaymentCompleted,
                orderStateInstance =>
                orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

            Event(() => PaymentFailed,
                orderStateInstance =>
                orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

            // Order Started ise Stock API da orderdaki Ürünlerin Stokları elimizde var mı yok mu kontrolü için OrderCreated Komutunu Stock API gönder.Süreci Stock API devret
            Initially(When(OrderSubmitted)
                .Then(context =>
                {
                    context.Instance.BuyerId = context.Data.BuyerId;
                    context.Instance.OrderId = context.Data.OrderId;
                    context.Instance.TotalPrice = context.Data.TotalPrice;
                    context.Instance.CreatedDate = DateTime.Now;
                })
                .Then(context => Console.WriteLine("Ara işlem 1"))
                .Then(context => Console.WriteLine("Ara işlem 2"))
                .TransitionTo(OrderSubmmitedState)
                .Then(context => Console.WriteLine("Ara işlem 3"))
                .Send(new Uri($"queue:{RabbitMQSettings.Stock_OrderCreatedEventQueue}"), context => new CheckStockCommand(context.Instance.CorrelationId)
                {
                    OrderId = context.Data.OrderId,
                    OrderItems = context.Data.OrderItems
                }));

      // Eğer OrderCreated State'indeyken STOCK APIDEN Stock Reserved Eventi fırlatıldıysa ORDER STATE MACHINE dan PAYMENT STARTED Eventi Fırlat. PAYMENT API İşi devret.
      During(OrderSubmmitedState,
          When(StockReserved)
          .TransitionTo(StockReservedState)
          .Send(new Uri($"queue:{RabbitMQSettings.Payment_StartedEventQueue}"), context => new StartPaymentCommand(context.Instance.CorrelationId)
          {
            OrderId = context.Instance.OrderId,
            TotalPrice = context.Instance.TotalPrice
          }),

          // STOCK NOTRESERVED ISE STOCK YETERSIZ ORDER IPTAL ET, ORDER API süreci devret. ORDER API Order State'i Failed olarak düzeltsin.
          When(StockNotReserved)
          .TransitionTo(StockNotReservedState)
          .Publish(context => new Shared.Events.OrderFailedEvent()
          {
            OrderId = context.Instance.OrderId,
            Message = context.Data.Message
          }));



      // Eğer STOCKRESERVED IKEN PAYMENT COMPLETED ISE ÖDEME ALINMIŞTIR, STATE MACHINE üzerinden ORDER API süreci devret. Order API da Order State'ini Completed olarak güncellesin. Burada tüm süreç başarılı bir şekilde sonlandı.
      During(StockReservedState,
          When(PaymentCompleted)
          .TransitionTo(PaymentCompletedState)
          .Publish(context => new OrderCompletedEvent
          {
            OrderId = context.Instance.OrderId
          }),
          //.Finalize(), // Burada işi finalize ediyoruz.

          // Eğer ödeme alınmadıysa bu durumda STATE Machine Order API order Failed komutu gönderip, ORDER API Order state İptale çeksin. Süreç burada sonlansın
          When(PaymentFailed)
          .TransitionTo(PaymentFailedState)
          .Publish(context => new Shared.Events.OrderFailedEvent()
          {
            OrderId = context.Instance.OrderId,
            Message = context.Data.Message

          }) // Süreç burada sonlasın

          ) ;
    }

    // Not State Machine aynı CorrelationId üzerinden state takibi yaparak, her bir state geçişini veri tabanına yansıtır. OrderCreated, StockReserverd, Final statelerinden geçiyor. Son state Finalize methodundan dolayı Final state olarak görüntülenir.
  }
}
