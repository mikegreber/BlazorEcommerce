using BlazorEcommerce.Client.Pages;
using BlazorEcommerce.Shared;

namespace BlazorEcommerce.Client.Services.CartService;

public class CartService : ICartService
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;
    private const string CartKey = "cart";
    private const string CartCountKey = "cartItemsCount";

    public CartService(ILocalStorageService localStorage, HttpClient httpClient, IAuthService authService)
    {
        _localStorage = localStorage;
        _httpClient = httpClient;
        _authService = authService;
        
    }

    public event Action? OnChange;

    public async Task AddToCart(CartItem cartItem)
    {
        if (await _authService.IsUserAuthenticated())
        {
            await _httpClient.PostAsJsonAsync("api/cart/add", cartItem);
        }
        else
        {
            var cart = await _localStorage.GetItemAsync<List<CartItem>>(CartKey) ?? new List<CartItem>();

            var existingItem = cart.Find(x => x.ProductId == cartItem.ProductId && x.ProductTypeId == cartItem.ProductTypeId);
            if (existingItem == null)
            {
                cart.Add(cartItem);
            }
            else
            {
                existingItem.Quantity += cartItem.Quantity;
            }

            await _localStorage.SetItemAsync(CartKey, cart);
        }

        await GetCartItemsCount();
    }

    public async Task<List<CartProductResponse>> GetCartProducts()
    {
        if (await _authService.IsUserAuthenticated())
        {
            var response = await _httpClient.GetFromJsonAsync<ServiceResponse<List<CartProductResponse>>>("api/cart");
            return response?.Data ?? new List<CartProductResponse>();
        }
        else
        {
            var cartItems = await _localStorage.GetItemAsync<List<CartItem>>(CartKey) ?? new List<CartItem>();
            var response = await _httpClient.PostAsJsonAsync("api/cart/products", cartItems);
            var cartProducts = await response.Content.ReadFromJsonAsync<ServiceResponse<List<CartProductResponse>>>();
            return cartProducts?.Data ?? new List<CartProductResponse>();
        }
    }

    public async Task RemoveProductFromCart(int productId, int productTypeId)
    {
        if (await _authService.IsUserAuthenticated())
        {
            await _httpClient.DeleteAsync($"api/cart/{productId}/{productTypeId}");
        }
        else
        {
            var cart = await _localStorage.GetItemAsync<List<CartItem>>(CartKey);
            if (cart == null) return;

            var cartItem = cart.Find(x => x.ProductId == productId && x.ProductTypeId == productTypeId);
            if (cartItem != null)
            {
                cart.Remove(cartItem);
                await _localStorage.SetItemAsync(CartKey, cart);
            }
        }

        await GetCartItemsCount();
    }

    public async Task UpdateQuantity(CartProductResponse product)
    {
        if (await _authService.IsUserAuthenticated())
        {
            var request = new CartItem()
            {
                ProductId = product.ProductId,
                Quantity = product.Quantity,
                ProductTypeId = product.ProductTypeId
            };

            await _httpClient.PutAsJsonAsync("api/cart/update-quantity", request);
        }
        else
        {
            var cart = await _localStorage.GetItemAsync<List<CartItem>>(CartKey);

            var cartItem = cart?.Find(x => x.ProductId == product.ProductId && x.ProductTypeId == product.ProductTypeId);
            if (cartItem != null)
            {
                cartItem.Quantity = product.Quantity;
                await _localStorage.SetItemAsync(CartKey, cart);
            }
        }
    }

    public async Task StoreCartItems(bool emptyLocalCart = false)
    {
        var cart = await _localStorage.GetItemAsync<List<CartItem>>(CartKey);
        if (cart == null) return;

        await _httpClient.PostAsJsonAsync("api/cart", cart);

        if (emptyLocalCart)
        {
            await _localStorage.RemoveItemAsync(CartKey);
        }
    }

    public async Task GetCartItemsCount()
    {
        if (await _authService.IsUserAuthenticated())
        {
            var result = await _httpClient.GetFromJsonAsync<ServiceResponse<int>>("api/cart/count");
            if (result != null)
            {
                var count = result.Data;
                await _localStorage.SetItemAsync(CartCountKey, count);
            }
        }
        else
        {
            var cart = await _localStorage.GetItemAsync<List<CartItem>>(CartKey);
            await _localStorage.SetItemAsync(CartCountKey, cart?.Count ?? 0);
        }

        OnChange?.Invoke();
    }
}