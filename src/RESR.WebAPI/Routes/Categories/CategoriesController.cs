using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RESR.Core.Controllers.Categories;
using RESR.Models.Categories;
using RESR.WebAPI.Security;

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

    [AuthorizePermission]
    [HttpGet("{idCategory:int}")]
    public async Task<ActionResult<CategoryResponse>> GetById([FromRoute] int idCategory, CancellationToken ct)
    {
        var category = await _service.GetByIdAsync(idCategory, ct);
        return category is null ? NotFound() : Ok(ToResponse(category));
    }

    [AuthorizePermission]
    [HttpPost("{idCategory:int}/favoriteCategory")]
    public async Task<ActionResult> AddToUser([FromRoute] int idCategory, CancellationToken ct)
    {
        var result = await _service.AddToUserAsync(idCategory, ct);
        return result switch
        {
            AddToUserResult.Added => NoContent(),
            AddToUserResult.AlreadyExists => Conflict(),
            AddToUserResult.NotFound => NotFound(),
            _ => StatusCode(StatusCodes.Status500InternalServerError),
        };
    }

    [AuthorizePermission]
    [HttpDelete("{idCategory:int}/favoriteCategory")]
    public async Task<ActionResult> RemoveFromUser([FromRoute] int idCategory, CancellationToken ct)
    {
        var success = await _service.RemoveFromUserAsync(idCategory, ct);
        return success ? NoContent() : NotFound();
    }

    private static CategoryResponse ToResponse(Category category) =>
        new(category.IdCategory, category.Name);
}
