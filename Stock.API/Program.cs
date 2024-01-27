using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stock.API.Services;
using System.Linq;
using MongoDB.Driver;

namespace Stock.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using IServiceScope scope = host.Services.CreateScope();
            MongoDbService mongoDbService = scope.ServiceProvider.GetRequiredService<MongoDbService>();
            if (!mongoDbService.GetCollection<Models.Stock>().FindSync(x => true).Any())
            {
                mongoDbService.GetCollection<Stock.API.Models.Stock>().InsertOne(new()
                {
                    ProductId = 21,
                    Count = 200
                });
                mongoDbService.GetCollection<Stock.API.Models.Stock>().InsertOne(new()
                {
                    ProductId = 22,
                    Count = 100
                });
                mongoDbService.GetCollection<Stock.API.Models.Stock>().InsertOne(new()
                {
                    ProductId = 23,
                    Count = 50
                });
                mongoDbService.GetCollection<Stock.API.Models.Stock>().InsertOne(new()
                {
                    ProductId = 24,
                    Count = 10
                });
                mongoDbService.GetCollection<Stock.API.Models.Stock>().InsertOne(new()
                {
                    ProductId = 25,
                    Count = 30
                });
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
