using MassTransit;
using MongoDB.Driver;
using Shared;
using Shared.Commands;
using Shared.Messages;
using Stock.API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stock.API.Consumers
{
    public class CheckStockCommandConsumer : IConsumer<CheckStockCommand>
  {
    readonly MongoDbService _mongoDbService;
    readonly ISendEndpointProvider _sendEndpointProvider;

    public CheckStockCommandConsumer(
        MongoDbService mongoDbService,
        ISendEndpointProvider sendEndpointProvider)
    {
      _mongoDbService = mongoDbService;
      _sendEndpointProvider = sendEndpointProvider;
    }

    public async Task Consume(ConsumeContext<CheckStockCommand> context)
    {


      IMongoCollection<Models.Stock> collection = _mongoDbService.GetCollection<Models.Stock>();

  

      // Her bir ürüne ait stok bilgisi siparişteki stock bilgisi ile check ediliyor.
      List<bool> stockResult = new();
      foreach (OrderItemMessage orderItem in context.Message.OrderItems)
        stockResult.Add((await collection.FindAsync(s => s.ProductId == orderItem.ProductId && s.Count > orderItem.Quantity)).Any());

      // sipariş edilen ürün stoklarından 1 tanesi bile uymaz ise stockNotReserved oluyor.
      // Hepsi uyarsa bu durumda stock reserved olarak kaydediliyor.
      
      ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.StateMachine}"));


      if (stockResult.TrueForAll(sr => sr.Equals(true)))
      {
        foreach (OrderItemMessage orderItem in context.Message.OrderItems)
        {
          Models.Stock stock = await (await collection.FindAsync(s => s.ProductId == orderItem.ProductId)).FirstOrDefaultAsync();
          stock.Reservations.Add(new Models.StockReservation
          {
            OrderId = context.Message.OrderId,
            Quantity = orderItem.Quantity,
            Applied = false
          });

          // Stock Reservation Kaydet
          // Payment işleminden sonra bu rezervasyonu silip Stock Count'tan düşeceğiz.
          await collection.FindOneAndReplaceAsync(x => x.ProductId == orderItem.ProductId, stock);
        }

        StockReservedCommand reserveStockCommand = new(context.Message.CorrelationId)
        {
          OrderId = context.Message.OrderId
        };
        await sendEndpoint.Send(reserveStockCommand);
      }
      else
      {
        StockNotreservedCommand NotReserveStockCommand = new(context.Message.CorrelationId)
        {
          Message = "Stok yetersiz...",
          OrderItems = context.Message.OrderItems
        };

        await sendEndpoint.Send(NotReserveStockCommand);
      }
    }
  }
}
