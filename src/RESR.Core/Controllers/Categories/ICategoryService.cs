using RESR.Models.Categories;

namespace RESR.Core.Controllers.Categories;

public interface ICategoryService
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct);
    Task<Category?> GetByIdAsync(int idCategory, CancellationToken ct);
    Task<AddToUserResult> AddToUserAsync(int idCategory, CancellationToken ct);
    Task<bool> RemoveFromUserAsync(int idCategory, CancellationToken ct);
}
