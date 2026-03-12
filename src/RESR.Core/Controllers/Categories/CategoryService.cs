using RESR.Core.Controllers.Categories.Ports;
using RESR.Models.Categories;

namespace RESR.Core.Controllers.Categories;

public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repo;

    public CategoryService(ICategoryRepository repo) => _repo = repo;

    public Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct) => _repo.GetAllAsync(ct);

    public Task<Category?> GetByIdAsync(int idCategory, CancellationToken ct) => _repo.GetByIdAsync(idCategory, ct);

    public Task<AddToUserResult> AddToUserAsync(int idCategory, CancellationToken ct) =>
        _repo.AddToUserAsync(idCategory, ct);

    public Task<bool> RemoveFromUserAsync(int idCategory, CancellationToken ct) => _repo.RemoveFromUserAsync(idCategory, ct);
}
