using System.Security.Claims;

namespace BlazorEcommerce.Server.Services.OrderService;

public class OrderService : IOrderService
{
    private readonly DataContext _context;
    private readonly ICartService _cartService;
    private readonly IAuthService _authService;
    

    public OrderService(DataContext context, ICartService cartService, IAuthService authService)
    {
        _context = context;
        _cartService = cartService;
        _authService = authService;
    }

    
    public async Task<ServiceResponse<bool>> PlaceOrder()
    {
        var order = new Order()
        {
            UserId = _authService.GetUserId(),
            OrderDate = DateTime.Now,
            TotalPrice = 0,
            OrderItems = new List<OrderItem>(),
        };

        var products = (await _cartService.GetDbCartProducts()).Data;

        foreach (var product in products)
        {
            var productPrice = product.Price * product.Quantity;
            order.TotalPrice += productPrice;
            order.OrderItems.Add(new OrderItem
            {
                ProductId = product.ProductId,
                ProductTypeId = product.ProductTypeId,
                Quantity = product.Quantity,
                TotalPrice = productPrice
            });
        }

        _context.Orders.Add(order);

        _context.CartItems.RemoveRange(_context.CartItems.Where(ci => ci.UserId == order.UserId));

        await _context.SaveChangesAsync();

        return new ServiceResponse<bool> { Data = true };
    }

    public async Task<ServiceResponse<List<OrderOverviewResponse>>> GetOrders()
    {
        var userId = _authService.GetUserId();
        var response = new ServiceResponse<List<OrderOverviewResponse>> { Data = new List<OrderOverviewResponse>() };
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        foreach (var order in orders)
        {
            response.Data.Add(new OrderOverviewResponse
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                TotalPrice = order.TotalPrice,
                Product = $"{order.OrderItems.First().Product.Title}" + (order.OrderItems.Count > 1 ? $" and {order.OrderItems.Count - 1} more..." : ""),
                ProductImageUrl = order.OrderItems.First().Product.ImageUrl
            });
        }

        return response;
    }

    public async Task<ServiceResponse<OrderDetailsResponse>> GetOrderDetails(int orderId)
    {
        var userId = _authService.GetUserId();

        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.ProductType)
            .Where(o => o.UserId == userId && o.Id == orderId)
            .OrderByDescending(o => o.OrderDate)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return new ServiceResponse<OrderDetailsResponse>
            {
                Success = false, 
                Message = "Order not found."
            };
        }

        var orderDetailsResponse = new OrderDetailsResponse
        {
            OrderDate = order.OrderDate,
            TotalPrice = order.TotalPrice,
            Products = new List<OrderDetailsProductResponse>()
        };

        order.OrderItems.ForEach(item => orderDetailsResponse.Products.Add(new OrderDetailsProductResponse()
        {
            ProductId = item.ProductId,
            ImageUrl = item.Product.ImageUrl,
            ProductType = item.ProductType.Name,
            Quantity = item.Quantity,
            Title = item.Product.Title,
            TotalPrice = item.TotalPrice
        }));

        return new ServiceResponse<OrderDetailsResponse> { Data = orderDetailsResponse };
    }
}