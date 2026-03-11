using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RESR.Core.Controllers.Categories;
using RESR.Models.Categories;

namespace RESR.WebAPI.Routes.Categories;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetAll(CancellationToken ct)
    {
        var categories = await _service.GetAllAsync(ct);
        return Ok(categories.Select(ToResponse).ToList());
    }

    [Authorize]
    [HttpGet("{idCategory:int}")]
    public async Task<ActionResult<CategoryResponse>> GetById([FromRoute] int idCategory, CancellationToken ct)
    {
        var category = await _service.GetByIdAsync(idCategory, ct);
        return category is null ? NotFound() : Ok(ToResponse(category));
    }

    private static CategoryResponse ToResponse(Category category) =>
        new(category.IdCategory, category.Name);
}
