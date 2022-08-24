using System.Net.Http.Json;
using BlazorEcommerce.Shared;
using Microsoft.AspNetCore.Components;

namespace BlazorEcommerce.Client.Shared;

public partial class ProductList
{
    [Inject] private IProductService ProductService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await ProductService.GetProducts();
    }
}