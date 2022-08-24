using System.Net.Http.Json;
using BlazorEcommerce.Shared;
using Microsoft.AspNetCore.Components;

namespace BlazorEcommerce.Client.Shared;

public partial class ProductList
{
    [Inject] private HttpClient HttpClient { get; set; }

    private static List<Product> Products = new();

    protected override async Task OnInitializedAsync()
    {
        var result = await HttpClient.GetFromJsonAsync<List<Product>>("api/Product");
       
        if (result != null)
            Products = result;
    }
}