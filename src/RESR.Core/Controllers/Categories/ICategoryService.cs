using RESR.Models.Categories;

namespace RESR.Core.Controllers.Categories;

public interface ICategoryService
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct);
    Task<Category?> GetByIdAsync(int idCategory, CancellationToken ct);
    Task<IReadOnlyList<Category>> GetFavoriteCategoriesAsync(int idUser, CancellationToken ct);
    Task<AddToUserResult> AddToUserAsync(int idUser, int idCategory, CancellationToken ct);
    Task<bool> RemoveFromUserAsync(int idUser, int idCategory, CancellationToken ct);
}
