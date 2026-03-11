using RESR.Core.Controllers.Categories.Factories;

namespace RESR.Core.Tests.Categories.Factories;

public sealed class CategoryFactoryTests
{
    [Fact]
    public void Create_BuildsCategory()
    {
        var factory = new CategoryFactory();

        var category = factory.Create(3, "Atelier");

        Assert.Equal(3, category.IdCategory);
        Assert.Equal("Atelier", category.Name);
    }
}
