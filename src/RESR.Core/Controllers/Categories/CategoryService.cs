using RESR.Core.Controllers.Categories.Ports;
using RESR.Models.Categories;

namespace RESR.Core.Controllers.Categories;

public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repo;

    public CategoryService(ICategoryRepository repo) => _repo = repo;

    public Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct) => _repo.GetAllAsync(ct);

    public Task<Category?> GetByIdAsync(int idCategory, CancellationToken ct) => _repo.GetByIdAsync(idCategory, ct);

    public Task<IReadOnlyList<Category>> GetFavoriteCategoriesAsync(int idUser, CancellationToken ct) =>
        _repo.GetFavoriteCategoriesAsync(idUser, ct);

    public async Task<AddToUserResult> AddToUserAsync(int idUser, int idCategory, CancellationToken ct)
    {
        if (await _repo.GetByIdAsync(idCategory, ct) is null)
            return AddToUserResult.NotFound;

        return await _repo.AddToUserAsync(idUser, idCategory, ct);
    }

    public Task<bool> RemoveFromUserAsync(int idUser, int idCategory, CancellationToken ct) =>
        _repo.RemoveFromUserAsync(idUser, idCategory, ct);
}
