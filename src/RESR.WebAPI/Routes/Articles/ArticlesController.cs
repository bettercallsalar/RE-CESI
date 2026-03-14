using Microsoft.AspNetCore.Mvc;
using RESR.Core.Controllers.Articles;
using RESR.Core.Controllers.Resources;
using RESR.Core.Controllers.Users.Ports;
using RESR.Core.Errors;
using RESR.Core.Security.Token;
using RESR.Models.Resources;
using RESR.WebAPI.Security;

namespace RESR.WebAPI.Routes.Articles;

[ApiController]
[Route("api/articles")]
public sealed class ArticlesController : AuthenticatedResourceControllerBase
{
    private readonly IArticleService _service;
    private readonly IUserRepository _users;

    public ArticlesController(IArticleService service, IUserRepository users, ITokenService tokenService)
        : base(tokenService)
    {
        _service = service;
        _users = users;
    }

    private const int MaxPageSize = 100;

    [HttpGet]
    public async Task<ActionResult<PaginatedArticlesResponse>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? keyword = null,
        [FromQuery] int? idUser = null,
        [FromQuery] int? idCategory = null,
        [FromQuery] DateTime? createdFrom = null,
        [FromQuery] DateTime? createdTo = null,
        CancellationToken ct = default)
    {
        if (page <= 0 || pageSize <= 0 || idUser is <= 0 || idCategory is <= 0)
            return BadRequest(new { message = "Page, PageSize, IdUser and IdCategory must be greater than 0." });
        if (pageSize > MaxPageSize) return BadRequest(new { message = $"PageSize cannot be greater than {MaxPageSize}." });

        var filters = new ArticleListingFilters(
            Keyword: keyword,
            Visibility: ResourceVisibility.PUBLIC,
            IdUser: idUser,
            IdCategory: idCategory,
            IsApproved: true,
            CreatedFrom: createdFrom,
            CreatedTo: createdTo,
            IncludeDeleted: false
        );

        var (articles, totalCount) = await _service.GetPaginatedAsync(page, pageSize, filters, ct);
        var items = await ToResponsesAsync(articles, ct);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize);

        return Ok(new PaginatedArticlesResponse(items, page, pageSize, totalCount, totalPages));
    }

    [AuthorizePermissionOrSelf("idUser")]
    [HttpGet("{idUser:int}/my-articles")]
    public async Task<ActionResult<PaginatedArticlesResponse>> GetMyArticles(
        [FromRoute] int idUser,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? keyword = null,
        [FromQuery] ResourceVisibility? visibility = null,
        [FromQuery] int? idCategory = null,
        [FromQuery] bool? isApproved = null,
        [FromQuery] DateTime? createdFrom = null,
        [FromQuery] DateTime? createdTo = null,
        CancellationToken ct = default)
    {
        if (page <= 0 || pageSize <= 0 || idCategory is <= 0)
            return BadRequest(new { message = "Page, PageSize and IdCategory must be greater than 0." });
        if (pageSize > MaxPageSize) return BadRequest(new { message = $"PageSize cannot be greater than {MaxPageSize}." });

        var filters = new ArticleListingFilters(
            Keyword: keyword,
            Visibility: visibility,
            IdUser: idUser,
            IdCategory: idCategory,
            IsApproved: isApproved,
            CreatedFrom: createdFrom,
            CreatedTo: createdTo,
            IncludeDeleted: true
        );

        var (articles, totalCount) = await _service.GetPaginatedAsync(page, pageSize, filters, ct);
        var items = await ToResponsesAsync(articles, ct);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize);

        return Ok(new PaginatedArticlesResponse(items, page, pageSize, totalCount, totalPages));
    }

    [HttpGet("{idResource:int}")]
    public async Task<ActionResult<ArticleResponse>> GetByResourceId([FromRoute] int idResource, CancellationToken ct)
    {
        var article = await _service.GetByResourceIdAsync(idResource, ct);

        if (article is null || article.DeletedAt is not null || article.Visibility != ResourceVisibility.PUBLIC || !article.IsApproved)
            return NotFound();

        return Ok(await ToResponseAsync(article, ct));
    }

    [HttpGet("me/{idResource:int}")]
    public async Task<ActionResult<ArticleResponse>> GetOwnByResourceId([FromRoute] int idResource, CancellationToken ct)
    {
        var (authResult, idUser) = await RequireAuthenticatedUserAsync(ct);
        if (authResult is not null)
            return authResult;

        var article = await _service.GetByResourceIdAsync(idResource, ct);

        if (article is null)
            return NotFound();

        if (article.IdUser != idUser)
            return Forbid();

        return Ok(await ToResponseAsync(article, ct));
    }

    [AuthorizePermission(PermissionNames.CreateResource)]
    [HttpPost]
    public async Task<ActionResult> Create([FromForm] CreateArticleFormRequest req, CancellationToken ct)
    {
        var visibility = Enum.Parse<ResourceVisibility>(req.Visibility, ignoreCase: true);
        var (authResult, idUser) = await RequireAuthenticatedUserAsync(ct);
        if (authResult is not null)
            return authResult;

        try
        {
            var idResource = await _service.CreateAsync(
                new CreateArticleCommand(
                    req.Title,
                    req.Description,
                    visibility,
                    idUser,
                    req.IdCategory,
                    req.Content,
                    await ToUploadsAsync(req.Images, ct),
                    req.DefaultImageIndex),
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
        [FromForm] UpdateArticleFormRequest req,
        CancellationToken ct)
    {
        var (authResult, idUser) = await RequireAuthenticatedUserAsync(ct);
        if (authResult is not null)
            return authResult;

        ResourceVisibility? visibility = req.Visibility is null
            ? null
            : Enum.Parse<ResourceVisibility>(req.Visibility, ignoreCase: true);

        try
        {
            var article = await _service.UpdateAsync(
                new UpdateArticleCommand(
                    IdResource: idResource,
                    IdUser: idUser,
                    Title: req.Title,
                    Description: req.Description,
                    Visibility: visibility,
                    IdCategory: req.IdCategory,
                    Content: req.Content,
                    Files: await ToUploadsAsync(req.Images, ct),
                    ReplaceFiles: req.ReplaceImages,
                    DefaultImageId: req.DefaultImageId,
                    DefaultImageIndex: req.DefaultImageIndex),
                ct);

            return Ok(await ToResponseAsync(article, ct));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ForbiddenException)
        {
            return Forbid();
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
        var (authResult, idUser) = await RequireAuthenticatedUserAsync(ct);
        if (authResult is not null)
            return authResult;

        try
        {
            var deleted = await _service.SoftDeleteAsync(idResource, idUser, ct);
            return deleted ? NoContent() : NotFound();
        }
        catch (ForbiddenException)
        {
            return Forbid();
        }
    }

    [AuthorizePermission(PermissionNames.ApproveArticle)]
    [HttpPatch("{idResource:int}/approval")]
    public async Task<ActionResult<ArticleResponse>> SetApproval(
        [FromRoute] int idResource,
        [FromBody] SetResourceApprovalRequest req,
        CancellationToken ct)
    {
        try
        {
            var article = await _service.SetApprovalAsync(
                new SetArticleApprovalCommand(idResource, req.IsApproved),
                ct);

            return Ok(await ToResponseAsync(article, ct));
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

    private async Task<List<ArticleResponse>> ToResponsesAsync(IEnumerable<Article> articles, CancellationToken ct)
    {
        var articleList = articles.ToList();
        var authorMap = await BuildAuthorMapAsync(articleList.Select(article => article.IdUser), ct);

        return articleList.Select(article => ToResponse(article, authorMap)).ToList();
    }

    private async Task<ArticleResponse> ToResponseAsync(Article article, CancellationToken ct)
    {
        var authorMap = await BuildAuthorMapAsync([article.IdUser], ct);
        return ToResponse(article, authorMap);
    }

    private async Task<Dictionary<int, ResourceAuthorResponse>> BuildAuthorMapAsync(IEnumerable<int> userIds, CancellationToken ct)
    {
        var ids = userIds.Distinct().ToList();
        var users = await Task.WhenAll(ids.Select(id => _users.GetByIdAsync(id, ct)));

        return users
            .Where(user => user is not null)
            .ToDictionary(
                user => user!.IdUser,
                user => new ResourceAuthorResponse(user!.IdUser, user.Username, user.FirstName));
    }

    private static ArticleResponse ToResponse(Article article, IReadOnlyDictionary<int, ResourceAuthorResponse> authorMap)
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
            article.DeletedAt,
            article.IdUser,
            authorMap.TryGetValue(article.IdUser, out var author)
                ? author
                : new ResourceAuthorResponse(article.IdUser, string.Empty, string.Empty),
            article.IdCategory,
            article.Content,
            article.IsApproved,
            article.DefaultImageId,
            article.Files.Select(ToFileResponse).ToList()
        );
    }

    private static ResourceFileResponse ToFileResponse(ResourceFile file) =>
        new(file.IdFile, file.FileName, file.OriginalName, file.MimeType, file.Size, file.Path, file.CreatedAt);

    private static async Task<IReadOnlyList<ResourceFileUpload>> ToUploadsAsync(IReadOnlyList<IFormFile>? files, CancellationToken ct)
    {
        if (files is null || files.Count == 0)
            return Array.Empty<ResourceFileUpload>();

        var uploads = new List<ResourceFileUpload>(files.Count);

        foreach (var file in files.Where(file => file.Length > 0))
        {
            await using var stream = file.OpenReadStream();
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory, ct);

            uploads.Add(new ResourceFileUpload(
                file.FileName,
                file.ContentType,
                Convert.ToInt32(file.Length),
                memory.ToArray()));
        }

        return uploads;
    }
}
