using Automatonymous;
using SagaStateMachine.Service.Instruments;
using Shared;
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
        public Event<OrderStartedEvent> OrderStartedEvent { get; set; }
        public Event<StockReservedEvent> StockReservedEvent { get; set; }
        public Event<PaymentCompletedEvent> PaymentCompletedEvent { get; set; }
        public Event<PaymentFailedEvent> PaymentFailedEvent { get; set; }
        public Event<StockNotReservedEvent> StockNotReservedEvent { get; set; }

    // Önemli: State Machine içerisinde sadece State Machine tarafından dinlenecek olan eventler tanımlanır. Dinleyici eventler durum değişikliklerinin yakalanması ve sürecin yönetilmesi için kullanılır.
    // Bu eventler süreci API devremek için farklı Eventleri Queue devredebilir. 

        public State OrderCreated { get; set; }
        public State StockReserved { get; set; }
        public State PaymentCompleted { get; set; }
        public State PaymentFailed { get; set; }
        public State StockNotReserved { get; set; }
        public OrderStateMachine()
        {
            //State Instance'da ki hangi property'nin sipariş sürecindeki state'i tutacağı bildiriliyor.
            //Yani artık tüm event'ler CurrentState property'sin de tutulacaktır!
            InstanceState(instance => instance.CurrentState);

      // Süreci Order Started Eventinden başladığımız için bir Corrlation yani Süreç takip Idsi ürettik. Diğer Eventler aynı Id üzerinden süreçlerini devam ettirecektir. aşığdaki  orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId)); kodu bunun için kullanılmıştır.

            Event(() => OrderStartedEvent,
                orderStateInstance =>
                orderStateInstance.CorrelateBy<int>(database => database.OrderId, @event => @event.Message.OrderId)
                .SelectId(e => Guid.NewGuid()));

            Event(() => StockReservedEvent,
                orderStateInstance =>
                orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

            Event(() => StockNotReservedEvent,
                orderStateInstance =>
                orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));
    
            Event(() => PaymentCompletedEvent,
                orderStateInstance =>
                orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

            Event(() => PaymentFailedEvent,
                orderStateInstance =>
                orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

            // Order Started ise Stock API da orderdaki Ürünlerin Stokları elimizde var mı yok mu kontrolü için OrderCreated Komutunu Stock API gönder.Süreci Stock API devret
            Initially(When(OrderStartedEvent)
                .Then(context =>
                {
                    context.Instance.BuyerId = context.Data.BuyerId;
                    context.Instance.OrderId = context.Data.OrderId;
                    context.Instance.TotalPrice = context.Data.TotalPrice;
                    context.Instance.CreatedDate = DateTime.Now;
                })
                .Then(context => Console.WriteLine("Ara işlem 1"))
                .Then(context => Console.WriteLine("Ara işlem 2"))
                .TransitionTo(OrderCreated)
                .Then(context => Console.WriteLine("Ara işlem 3"))
                .Send(new Uri($"queue:{RabbitMQSettings.Stock_OrderCreatedEventQueue}"), context => new OrderCreatedEvent(context.Instance.CorrelationId)
                {
                    OrderId = context.Data.OrderId,
                    OrderItems = context.Data.OrderItems
                }));

      // Eğer OrderCreated State'indeyken STOCK APIDEN Stock Reserved Eventi fırlatıldıysa ORDER STATE MACHINE dan PAYMENT STARTED Eventi Fırlat. PAYMENT API İşi devret.
      During(OrderCreated,
          When(StockReservedEvent)
          .TransitionTo(StockReserved)
          .Send(new Uri($"queue:{RabbitMQSettings.Payment_StartedEventQueue}"), context => new PaymentStartedEvent(context.Instance.CorrelationId)
          {
            OrderId = context.Instance.OrderId,
            TotalPrice = context.Instance.TotalPrice
          }),

          // STOCK NOTRESERVED ISE STOCK YETERSIZ ORDER IPTAL ET, ORDER API süreci devret. ORDER API Order State'i Failed olarak düzeltsin.
          When(StockNotReservedEvent)
          .TransitionTo(StockNotReserved)
          .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderFailedEventQueue}"), context => new OrderFailedEvent()
          {
            OrderId = context.Instance.OrderId,
            Message = context.Data.Message
          }));



      // Eğer STOCKRESERVED IKEN PAYMENT COMPLETED ISE ÖDEME ALINMIŞTIR, STATE MACHINE üzerinden ORDER API süreci devret. Order API da Order State'ini Completed olarak güncellesin. Burada tüm süreç başarılı bir şekilde sonlandı.
      During(StockReserved,
          When(PaymentCompletedEvent)
          .TransitionTo(PaymentCompleted)
          .Publish(context => new OrderCompletedEvent
          {
            OrderId = context.Instance.OrderId
          }),
          //.Finalize(), // Burada işi finalize ediyoruz.

          // Eğer ödeme alınmadıysa bu durumda STATE Machine Order API order Failed komutu gönderip, ORDER API Order state İptale çeksin. Süreç burada sonlansın
          When(PaymentFailedEvent)
          .TransitionTo(PaymentFailed)
          .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderFailedEventQueue}"), context => new OrderFailedEvent()
          {
            OrderId = context.Instance.OrderId,
            Message = context.Data.Message

          }) // Süreç burada sonlasın

          ) ;
    }

    // Not State Machine aynı CorrelationId üzerinden state takibi yaparak, her bir state geçişini veri tabanına yansıtır. OrderCreated, StockReserverd, Final statelerinden geçiyor. Son state Finalize methodundan dolayı Final state olarak görüntülenir.
  }
}
