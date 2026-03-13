using RESR.Models.Categories;
using RESR.Core.Controllers.Categories;
namespace RESR.Core.Controllers.Categories.Ports;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct);
    Task<Category?> GetByIdAsync(int idCategory, CancellationToken ct);
    Task<IReadOnlyList<Category>> GetFavoriteCategoriesAsync(int idUser, CancellationToken ct);
    Task<AddToUserResult> AddToUserAsync(int idUser, int idCategory, CancellationToken ct);
    Task<bool> RemoveFromUserAsync(int idUser, int idCategory, CancellationToken ct);
}
