using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlazorEcommerce.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<ServiceResponse<List<OrderOverviewResponse>>>> GetOrders()
    {
        var result = await _orderService.GetOrders();
        return Ok(result);
    }

    [HttpGet("{orderId}")]
    public async Task<ActionResult<ServiceResponse<OrderDetailsResponse>>> GetOrderDetails(int orderId)
    {
        var result = await _orderService.GetOrderDetails(orderId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResponse<bool>>> PlaceOrder()
    {
        var result = await _orderService.PlaceOrder();
        return Ok(result);
    }
}