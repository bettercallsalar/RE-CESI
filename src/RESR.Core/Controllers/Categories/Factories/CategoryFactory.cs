using RESR.Models.Categories;

namespace RESR.Core.Controllers.Categories.Factories;

public sealed class CategoryFactory : ICategoryFactory
{
    public Category Create(int idCategory, string name) =>
        new()
        {
            IdCategory = idCategory,
            Name = name
        };
}
