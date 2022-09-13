namespace BlazorEcommerce.Client.Services.CategoryService;

public class CategoryService : ICategoryService
{
    private readonly HttpClient _httpClient;

    public CategoryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public event Action? OnChange;
    public List<Category> Categories { get; set; } = new();
    public List<Category> AdminCategories { get; set; } = new();

    public async Task GetCategories()
    {
        var response = await _httpClient.GetFromJsonAsync<ServiceResponse<List<Category>>>("api/Category");
        if (response?.Data != null)
            Categories = response.Data;
    }

    public async Task GetAdminCategories()
    {
        var response = await _httpClient.GetFromJsonAsync<ServiceResponse<List<Category>>>("api/Category/admin");
        if (response?.Data != null)
            AdminCategories = response.Data;
    }

    public async Task AddCategory(Category category)
    {
        var response = await _httpClient.PostAsJsonAsync("api/category/admin", category);
        AdminCategories = (await response.Content.ReadFromJsonAsync<ServiceResponse<List<Category>>>())?.Data ?? AdminCategories;
        await GetCategories();
        OnChange?.Invoke();
    }

    public async Task UpdateCategory(Category category)
    {
        var response = await _httpClient.PutAsJsonAsync("api/category/admin", category);
        AdminCategories = (await response.Content.ReadFromJsonAsync<ServiceResponse<List<Category>>>())?.Data ?? AdminCategories;
        await GetCategories();
        OnChange?.Invoke();
    }

    public async Task DeleteCategory(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/category/admin/{id}");
        AdminCategories = (await response.Content.ReadFromJsonAsync<ServiceResponse<List<Category>>>())?.Data ?? AdminCategories;
        await GetCategories();
        OnChange?.Invoke();
    }

    public Category CreateNewCategory()
    {
        var category = new Category { IsNew = true, Editing = true };
        AdminCategories.Add(category);
        OnChange?.Invoke();
        return category;
    }
}