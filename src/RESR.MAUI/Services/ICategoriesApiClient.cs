using RESR.Models.Categories;

namespace RESR.MAUI.Services;

public interface ICategoriesApiClient
{
    Task<IReadOnlyList<CategoryResponse>> GetCategoriesAsync(CancellationToken ct);
}
