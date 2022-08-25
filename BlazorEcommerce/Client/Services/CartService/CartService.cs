using BlazorEcommerce.Shared;

namespace BlazorEcommerce.Client.Services.CartService;

public class CartService : ICartService
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _httpClient;
    private readonly string _cartKey = "cart";

    public CartService(ILocalStorageService localStorage, HttpClient httpClient)
    {
        _localStorage = localStorage;
        _httpClient = httpClient;
    }

    public event Action? OnChange;
    public async Task AddToCart(CartItem cartItem)
    {
        var cart = await _localStorage.GetItemAsync<List<CartItem>>(_cartKey) ?? new List<CartItem>();

        var sameItem = cart.Find(x => x.ProductId == cartItem.ProductId && x.ProductTypeId == cartItem.ProductTypeId);
        if (sameItem == null)
        {
            cart.Add(cartItem);
        }
        else
        {
            sameItem.Quantity += cartItem.Quantity;
        }

        await _localStorage.SetItemAsync(_cartKey, cart);

        OnChange?.Invoke();
    }

    public async Task<List<CartItem>> GetCartItems()
    {
        return await _localStorage.GetItemAsync<List<CartItem>>(_cartKey) ?? new List<CartItem>();
    }

    public async Task<List<CartProductResponse>> GetCartProducts()
    {
        var cartItems = await _localStorage.GetItemAsync<List<CartItem>>(_cartKey);
        var response = await _httpClient.PostAsJsonAsync("api/cart/products", cartItems);
        var cartProducts = await response.Content.ReadFromJsonAsync<ServiceResponse<List<CartProductResponse>>>();
        return cartProducts.Data;
    }

    public async Task RemoveProductFromCart(int productId, int productTypeId)
    {
        var cart = await _localStorage.GetItemAsync<List<CartItem>>(_cartKey);
        if (cart == null) return;

        var cartItem = cart.Find(x => x.ProductId == productId && x.ProductTypeId == productTypeId);
        if (cartItem != null)
        {
            cart.Remove(cartItem);
            await _localStorage.SetItemAsync(_cartKey, cart);
            OnChange?.Invoke();
        }
            
    }

    public async Task UpdateQuantity(CartProductResponse product)
    {
        var cart = await _localStorage.GetItemAsync<List<CartItem>>(_cartKey);
        if (cart == null) return;

        var cartItem = cart.Find(x => x.ProductId == product.ProductId && x.ProductTypeId == product.ProductTypeId);
        if (cartItem != null)
        {
            cartItem.Quantity = product.Quantity;
            await _localStorage.SetItemAsync(_cartKey, cart);
        }
    }
}