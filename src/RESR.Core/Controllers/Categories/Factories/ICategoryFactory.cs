using RESR.Models.Categories;

namespace RESR.Core.Controllers.Categories.Factories;

public interface ICategoryFactory
{
    Category Create(int idCategory, string name);
}
