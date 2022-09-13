namespace BlazorEcommerce.Client.Services.ProductTypeService
{
    public class ProductTypeService : IProductTypeService
    {
        private readonly HttpClient _httpClient;

        public ProductTypeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public event Action? OnChange;
        public List<ProductType> ProductTypes { get; set; } = new();
        public async Task GetProductTypes()
        {
            var result = await _httpClient.GetFromJsonAsync<ServiceResponse<List<ProductType>>>("api/ProductType");
            if (result?.Data != null) 
                ProductTypes = result.Data;
            OnChange?.Invoke();
        }

        public async Task AddProductType(ProductType productType)
        {
            var response = await _httpClient.PostAsJsonAsync("api/producttype", productType);
            ProductTypes = (await response.Content.ReadFromJsonAsync<ServiceResponse<List<ProductType>>>())?.Data ?? ProductTypes;
            OnChange?.Invoke();
        }

        public async Task UpdateProductType(ProductType productType)
        {
            var response = await _httpClient.PutAsJsonAsync("api/producttype", productType);
            ProductTypes = (await response.Content.ReadFromJsonAsync<ServiceResponse<List<ProductType>>>())?.Data ?? ProductTypes;
            OnChange?.Invoke();
        }

        public ProductType CreateNewProductType()
        {
            var productType = new ProductType { IsNew = true, Editing = true };
            ProductTypes.Add(productType);
            OnChange?.Invoke();
            return productType;
        }
    }
}
