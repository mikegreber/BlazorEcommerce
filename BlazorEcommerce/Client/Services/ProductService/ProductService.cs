namespace BlazorEcommerce.Client.Services.ProductService;

public class ProductService : IProductService
{
    private readonly HttpClient _httpClient;

    public ProductService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public List<Product> Products { get; set; } = new List<Product>();

    public event Action? ProductsChanged;

    public async Task GetProducts(string? categoryUrl)
    {
        var result = categoryUrl == null ? 
            await _httpClient.GetFromJsonAsync<ServiceResponse<List<Product>>>("api/Product") :
            await _httpClient.GetFromJsonAsync<ServiceResponse<List<Product>>>($"api/Product/category/{categoryUrl}");
        if (result?.Data != null)
            Products = result.Data;

        ProductsChanged?.Invoke();
    }

    public async Task<ServiceResponse<Product>> GetProduct(int id)
    {
        var result = await _httpClient.GetFromJsonAsync<ServiceResponse<Product>>($"api/Product/{id}");
        return result;
    }
}