using MassTransit;
using MongoDB.Driver;
using Shared.Events;
using Stock.API.Models;
using Stock.API.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Stock.API.Consumers
{
  public class OrderCompletedEventConsumer : IConsumer<OrderCompletedEvent>
  {
    readonly MongoDbService _mongoDbService;

    public OrderCompletedEventConsumer(MongoDbService mongoDbService)
    {
      _mongoDbService = mongoDbService;
    }

    public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
    {
      // orderApplied'ında OrderId şu olan stockları getir

      var filters = Builders<Models.Stock>.Filter.ElemMatch(x => x.Reservations, reservation => reservation.OrderId == context.Message.OrderId && reservation.Applied == false);

     var stocks = _mongoDbService.GetCollection<Models.Stock>().Find(filters).ToList();

      stocks.ForEach((item) =>
      {
        var orderedReservations = item.Reservations.Where(x => x.OrderId == context.Message.OrderId && x.Applied == false);

        var orderedReservationCount = orderedReservations.Sum(x => x.Quantity);
        item.Count -= orderedReservations.Sum(x => x.Quantity);

        foreach (var reservation in orderedReservations)
        {
          reservation.Applied = true;
        }
        
        _mongoDbService.GetCollection<Models.Stock>().ReplaceOne(x => x.Id == item.Id, item);
      });

      await Task.CompletedTask;
    }
  }
}
