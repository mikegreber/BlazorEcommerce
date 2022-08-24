using System.Net.Http.Json;
using BlazorEcommerce.Shared;
using Microsoft.AspNetCore.Components;

namespace BlazorEcommerce.Client.Shared;

public partial class ProductList : IDisposable
{
    [Inject] private IProductService ProductService { get; set; } = null!;

    protected override void OnInitialized()
    {
        ProductService.ProductsChanged += StateHasChanged;
    }

    public void Dispose() => ProductService.ProductsChanged -= StateHasChanged;

    private string GetPriceText(Product product)
    {
        var variants = product.Variants;
        
        if (variants.Count == 0)
            return string.Empty;

        if (variants.Count == 1)
            return $"${variants[0].Price}";

        decimal minPrice = variants.Min(v => v.Price);
        return $"Starting at ${minPrice}";

    }
}