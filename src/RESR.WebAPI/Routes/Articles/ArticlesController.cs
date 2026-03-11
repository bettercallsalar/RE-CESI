using Microsoft.AspNetCore.Mvc;
using RESR.Core.Controllers.Articles;
using RESR.Core.Errors;
using RESR.Models.Resources;
using RESR.WebAPI.Security;

namespace RESR.WebAPI.Routes.Articles;

[ApiController]
[Route("api/articles")]
public sealed class ArticlesController : ControllerBase
{
    private readonly IArticleService _service;

    public ArticlesController(IArticleService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ArticleResponse>>> GetAll(CancellationToken ct)
    {
        var articles = await _service.GetAllAsync(ct);
        return Ok(articles.Select(ToResponse).ToList());
    }

    [HttpGet("{idResource:int}")]
    public async Task<ActionResult<ArticleResponse>> GetByResourceId([FromRoute] int idResource, CancellationToken ct)
    {
        var article = await _service.GetByResourceIdAsync(idResource, ct);
        return article is null ? NotFound() : Ok(ToResponse(article));
    }

    [AuthorizePermission(PermissionNames.CreateResource)]
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateArticleRequest req, CancellationToken ct)
    {
        var visibility = Enum.Parse<ResourceVisibility>(req.Visibility, ignoreCase: true);

        try
        {
            var idResource = await _service.CreateAsync(
                new CreateArticleCommand(
                    req.Title,
                    req.Description,
                    visibility,
                    req.IdUser,
                    req.IdCategory,
                    req.Content),
                ct);

            return CreatedAtAction(nameof(GetByResourceId), new { idResource }, new { idResource });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [AuthorizePermission(PermissionNames.EditResource)]
    [HttpPatch("{idResource:int}")]
    public async Task<ActionResult<ArticleResponse>> Update(
        [FromRoute] int idResource,
        [FromBody] UpdateArticleRequest req,
        CancellationToken ct)
    {
        ResourceVisibility? visibility = req.Visibility is null
            ? null
            : Enum.Parse<ResourceVisibility>(req.Visibility, ignoreCase: true);

        try
        {
            var article = await _service.UpdateAsync(
                new UpdateArticleCommand(
                    idResource,
                    req.Title,
                    req.Description,
                    visibility,
                    req.IdCategory,
                    req.Content),
                ct);

            return Ok(ToResponse(article));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [AuthorizePermission(PermissionNames.DeleteResource)]
    [HttpDelete("{idResource:int}")]
    public async Task<ActionResult> Delete([FromRoute] int idResource, CancellationToken ct)
    {
        var deleted = await _service.SoftDeleteAsync(idResource, ct);
        return deleted ? NoContent() : NotFound();
    }

    [AuthorizePermission(PermissionNames.ApproveArticle)]
    [HttpPatch("{idResource:int}/approval")]
    public async Task<ActionResult<ArticleResponse>> SetApproval(
        [FromRoute] int idResource,
        [FromBody] SetArticleApprovalRequest req,
        CancellationToken ct)
    {
        try
        {
            var article = await _service.SetApprovalAsync(
                new SetArticleApprovalCommand(idResource, req.IsApproved),
                ct);

            return Ok(ToResponse(article));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private static ArticleResponse ToResponse(Article article)
    {
        return new ArticleResponse(
            article.IdResource,
            article.IdArticle,
            article.Title,
            article.Description,
            article.Type.ToString().ToLowerInvariant(),
            article.Visibility.ToString(),
            article.CreatedAt,
            article.ModifiedAt,
            article.IdUser,
            article.IdCategory,
            article.Content,
            article.IsApproved
        );
    }
}
