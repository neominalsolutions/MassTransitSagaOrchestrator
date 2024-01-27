using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SagaStateMachine.Service.Instruments;
using SagaStateMachine.Service.StateMachines;
using Shared;

namespace SagaStateMachine.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMassTransit(configure =>
                    {
                        configure.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>()
                          .EntityFrameworkRepository(options =>
                          {
                              options.AddDbContext<DbContext, OrderStateDbContext>((provider, builder) =>
                              {
                                  builder.UseSqlServer(hostContext.Configuration.GetConnectionString("SQLServer"));
                              });
                          });

                        configure.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                        {
                            cfg.Host(hostContext.Configuration.GetConnectionString("RabbitMQLocal"));

                            cfg.ReceiveEndpoint(RabbitMQSettings.StateMachine, e =>
                            e.ConfigureSaga<OrderStateInstance>(provider));
                        }));
                    });

                    services.AddMassTransitHostedService();

                });
    }
}