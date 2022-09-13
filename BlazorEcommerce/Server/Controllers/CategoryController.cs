using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorEcommerce.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Category>>> GetCategories()
    {
        var result = await _categoryService.GetCategoriesAsync();
        return Ok(result);
    }

    [HttpGet("admin"), Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<Category>>> GetAdminCategories()
    {
        var result = await _categoryService.GetAdminCategories();
        return Ok(result);
    }

    [HttpPost("admin"), Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<Category>>> AddCategory(Category category)
    {
        var result = await _categoryService.AddCategory(category);
        return Ok(result);
    }

    [HttpPut("admin"), Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<Category>>> UpdateCategory(Category category)
    {
        var result = await _categoryService.UpdateCategory(category);
        return Ok(result);
    }

    [HttpDelete("admin/{id}"), Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<Category>>> DeleteCategories(int id)
    {
        var result = await _categoryService.DeleteCategory(id);
        return Ok(result);
    }
}