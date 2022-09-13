using System.Runtime.InteropServices;

namespace BlazorEcommerce.Client.Services.ProductService;

public class ProductService : IProductService
{
    private readonly HttpClient _httpClient;

    public ProductService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public List<Product> Products { get; set; } = new();
    public List<Product> AdminProducts { get; set; }
    public string Message { get; set; } = "Loading Products...";
    public int CurrentPage { get; set; } = 1;
    public int PageCount { get; set; } = 0;
    public string LastSearchText { get; set; } = string.Empty;

    public event Action? ProductsChanged;

    public async Task GetProducts(string? categoryUrl)
    {
        var result = categoryUrl == null ? 
            await _httpClient.GetFromJsonAsync<ServiceResponse<List<Product>>>("api/product/featured") :
            await _httpClient.GetFromJsonAsync<ServiceResponse<List<Product>>>($"api/product/category/{categoryUrl}");
        
        if (result?.Data != null)
            Products = result.Data;

        if (Products.Count == 0)
            Message = "No products found.";

        CurrentPage = 1;
        PageCount = 0;
        
        ProductsChanged?.Invoke();
    }

    public async Task GetAdminProducts()
    {
        var result = await _httpClient.GetFromJsonAsync<ServiceResponse<List<Product>>>("api/product/admin");
        if (result?.Data != null)
            AdminProducts = result.Data;
        CurrentPage = 1;
        PageCount = 0;
        if (AdminProducts.Count == 0)
            Message = "No products found";
    }

    public async Task<ServiceResponse<Product>> GetProduct(int id)
    {
        var result = await _httpClient.GetFromJsonAsync<ServiceResponse<Product>>($"api/product/{id}");
        return result;
    }

    public async Task SearchProducts(string searchText, int page)
    {
        LastSearchText = searchText;

        var result = await _httpClient.GetFromJsonAsync<ServiceResponse<ProductSearchResult>>($"api/product/search/{searchText}/{page}");
        
        if (result?.Data != null)
        {
            Products = result.Data.Products;
            CurrentPage = result.Data.CurrentPage;
            PageCount = result.Data.Pages;
        }

        if (Products.Count == 0)
            Message = "No products found.";

        ProductsChanged?.Invoke();
    }


    public async Task<List<string>> GetProductSearchSuggestions(string searchText)
    {
        var result =
            await _httpClient.GetFromJsonAsync<ServiceResponse<List<string>>>(
                $"api/product/searchsuggestions/{searchText}");

        return result?.Data ?? new List<string>();
    }

    public async Task<Product?> CreateProduct(Product product)
    {
        var result = await _httpClient.PostAsJsonAsync("api/product", product);
        return (await result.Content.ReadFromJsonAsync<ServiceResponse<Product>>())?.Data;
    }

    public async Task<Product?> UpdateProduct(Product product)
    {
        var result = await _httpClient.PutAsJsonAsync("api/product", product);
        return (await result.Content.ReadFromJsonAsync<ServiceResponse<Product>>())?.Data;
    }

    public async Task DeleteProduct(Product product)
    {
        var result = await _httpClient.DeleteAsync($"api/product/{product.Id}");
    }
}