using RESR.Models.Categories;
using RESR.Core.Controllers.Categories;
namespace RESR.Core.Controllers.Categories.Ports;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct);
    Task<Category?> GetByIdAsync(int idCategory, CancellationToken ct);
    Task<AddToUserResult> AddToUserAsync(int idCategory, CancellationToken ct);
    Task<bool> RemoveFromUserAsync(int idCategory, CancellationToken ct);
}
