using Moq;
using RESR.Core.Controllers.Categories;
using RESR.Core.Controllers.Categories.Ports;
using RESR.Models.Categories;

namespace RESR.Core.Tests.Categories;

public sealed class CategoryServiceTests
{
    [Fact]
    public async Task GetAllAsync_DelegatesToRepository()
    {
        var repo = new Mock<ICategoryRepository>();
        repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category> { new() { IdCategory = 1, Name = "Conference" } });

        var service = new CategoryService(repo.Object);

        var list = await service.GetAllAsync(CancellationToken.None);

        Assert.Single(list);
        repo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_DelegatesToRepository()
    {
        var repo = new Mock<ICategoryRepository>();
        repo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category { IdCategory = 2, Name = "Salon" });

        var service = new CategoryService(repo.Object);

        var category = await service.GetByIdAsync(2, CancellationToken.None);

        Assert.NotNull(category);
        Assert.Equal(2, category!.IdCategory);
        repo.Verify(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()), Times.Once);
    }
}
