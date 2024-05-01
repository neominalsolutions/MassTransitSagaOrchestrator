using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.Models;
using Order.API.Models.Contexts;
using Order.API.Models.Enums;
using Order.API.ViewModels;
using Shared;
using Shared.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        readonly ApplicationDbContext _applicationDbContext;
        readonly ISendEndpointProvider _sendEndpointProvider;
        public OrdersController(ApplicationDbContext applicationDbContext,
            ISendEndpointProvider sendEndpointProvider)
        {
            _applicationDbContext = applicationDbContext;
            _sendEndpointProvider = sendEndpointProvider;
        }
        [HttpPost]
        public async Task<IActionResult> SubmitOrder(OrderVM model)
        {
            Order.API.Models.Order order = new()
            {
                BuyerId = model.BuyerId,
                OrderItems = model.OrderItems.Select(oi => new OrderItem
                {
                    Count = oi.Count,
                    Price = oi.Price,
                    ProductId = oi.ProductId
                }).ToList(),
                OrderStatus = OrderStatus.Suspend,
                TotalPrice = model.OrderItems.Sum(oi => oi.Count * oi.Price),
                CreatedDate = DateTime.Now
            };

            await _applicationDbContext.AddAsync<Order.API.Models.Order>(order);

            await _applicationDbContext.SaveChangesAsync();

            SubmitOrderCommand submitOrder = new()
            {
                BuyerId = model.BuyerId,
                OrderId = order.Id,
                TotalPrice = model.OrderItems.Sum(oi => oi.Count * oi.Price),
                OrderItems = model.OrderItems.Select(oi => new Shared.Messages.OrderItemMessage
                {
                    Price = oi.Price,
                    Quantity = oi.Count,
                    ProductId = oi.ProductId
                }).ToList()
            };

            ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new($"queue:{RabbitMQSettings.StateMachine}"));
            await sendEndpoint.Send(submitOrder);
            return Ok(true);
        }
    }
}
