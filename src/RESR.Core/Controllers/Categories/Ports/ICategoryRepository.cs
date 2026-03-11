using RESR.Models.Categories;
namespace RESR.Core.Controllers.Categories.Ports;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct);
    Task<Category?> GetByIdAsync(int idCategory, CancellationToken ct);
}
