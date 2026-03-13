using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RESR.Core.Controllers.Categories;
using RESR.Core.Security.Token;
using RESR.Models.Categories;
using RESR.WebAPI.Security;

namespace RESR.WebAPI.Routes.Categories;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController : AuthenticatedResourceControllerBase
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service, ITokenService tokenService)
        : base(tokenService)
    {
        _service = service;
    }

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
    [HttpGet("favoriteCategory")]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetFavoriteCategories(CancellationToken ct)
    {
        var authResult = RequireAuthenticatedUser(out var idUser);
        if (authResult is not null)
            return authResult;

        var categories = await _service.GetFavoriteCategoriesAsync(idUser, ct);
        return Ok(categories.Select(ToResponse).ToList());
    }

    [AuthorizePermission]
    [HttpPost("{idCategory:int}/favoriteCategory")]
    public async Task<ActionResult> AddToUser([FromRoute] int idCategory, CancellationToken ct)
    {
        var authResult = RequireAuthenticatedUser(out var idUser);
        if (authResult is not null)
            return authResult;

        var result = await _service.AddToUserAsync(idUser, idCategory, ct);
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
        var authResult = RequireAuthenticatedUser(out var idUser);
        if (authResult is not null)
            return authResult;

        var success = await _service.RemoveFromUserAsync(idUser, idCategory, ct);
        return success ? NoContent() : NotFound();
    }

    private static CategoryResponse ToResponse(Category category) =>
        new(category.IdCategory, category.Name);
}
