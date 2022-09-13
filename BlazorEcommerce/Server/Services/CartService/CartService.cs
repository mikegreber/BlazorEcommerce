using BlazorEcommerce.Shared;
using System.Security.Claims;

namespace BlazorEcommerce.Server.Services.CartService;

public class CartService : ICartService
{
    private readonly DataContext _context;
    private readonly IAuthService _authService;

    public CartService(DataContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
       
    }

    public async Task<ServiceResponse<List<CartProductResponse>>> GetCartProducts(List<CartItem> cartItems)
    {
        var result = new ServiceResponse<List<CartProductResponse>>
        {
            Data = new List<CartProductResponse>()
        };

        foreach (var item in cartItems)
        {
            var product = await _context.Products
                .Where(p => p.Id == item.ProductId)
                .FirstOrDefaultAsync();

            if (product == null) continue;


            var productVariant = await _context.ProductVariants
                .Where(v => v.ProductId == item.ProductId && v.ProductTypeId == item.ProductTypeId)
                .Include(v => v.ProductType)
                .FirstOrDefaultAsync();

            if (productVariant == null) continue;


            var cartProduct = new CartProductResponse
            {
                ProductId = product.Id,
                Title = product.Title,
                ImageUrl = product.ImageUrl,
                Price = productVariant.Price,
                ProductType = productVariant.ProductType.Name,
                ProductTypeId = productVariant.ProductTypeId,
                Quantity = item.Quantity
            };

            result.Data.Add(cartProduct);
        }

        return result;
    }

    public async Task<ServiceResponse<List<CartProductResponse>>> StoreCartItems(List<CartItem> cartItems)
    {
        var userId = _authService.GetUserId();

        cartItems.ForEach(cartItem => cartItem.UserId = userId);

        _context.CartItems.AddRange(cartItems);
        await _context.SaveChangesAsync();

        return await GetDbCartProducts();
    }

    public async Task<ServiceResponse<int>> GetCartItemsCount()
    {
        var userId = _authService.GetUserId();
        var count = (await _context.CartItems.Where(ci => ci.UserId == userId).ToListAsync()).Count;

        return new ServiceResponse<int>() { Data = count };
    }

    public async Task<ServiceResponse<List<CartProductResponse>>> GetDbCartProducts(int? userId = null)
    {
        userId ??= _authService.GetUserId();
        return await GetCartProducts(await _context.CartItems.Where(ci => ci.UserId == userId).ToListAsync());
    }

    public async Task<ServiceResponse<bool>> AddToCart(CartItem cartItem)
    {
        cartItem.UserId = _authService.GetUserId();

        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(ci =>
                ci.UserId == cartItem.UserId && 
                ci.ProductId == cartItem.ProductId &&
                ci.ProductTypeId == cartItem.ProductTypeId);

        if (existingItem == null)
        {
            _context.CartItems.Add(cartItem);
        }
        else
        {
            existingItem.Quantity += cartItem.Quantity;
        }

        await _context.SaveChangesAsync();

        return new ServiceResponse<bool> { Data = true };
    }

    public async Task<ServiceResponse<bool>> UpdateQuantity(CartItem cartItem)
    {
        cartItem.UserId = _authService.GetUserId();

        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(ci =>
                ci.UserId == cartItem.UserId &&
                ci.ProductId == cartItem.ProductId &&
                ci.ProductTypeId == cartItem.ProductTypeId);

        if (existingItem == null)
        {
            return new ServiceResponse<bool>
            {
                Data = false,
                Message = "Cart item does not exist.",
                Success = false
            };
        }

        existingItem.Quantity = cartItem.Quantity;
        await _context.SaveChangesAsync();

        return new ServiceResponse<bool> { Data = true };

    }

    public async Task<ServiceResponse<bool>> RemoveItemFromCart(int productId, int productTypeId)
    {
        var userId = _authService.GetUserId();

        var existingItem = await _context.CartItems
        .FirstOrDefaultAsync(ci =>
                ci.UserId == userId &&
                ci.ProductId == productId &&
                ci.ProductTypeId == productTypeId);

        if (existingItem == null)
        {
            return new ServiceResponse<bool>
            {
                Data = false,
                Message = "Cart item does not exist.",
                Success = false
            };
        }

        _context.CartItems.Remove(existingItem);

        await _context.SaveChangesAsync();

        return new ServiceResponse<bool> { Data = true };
    }
}