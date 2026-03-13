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

    [Fact]
    public async Task AddToUserAsync_ReturnsNotFound_WhenCategoryMissing()
    {
        var repo = new Mock<ICategoryRepository>();
        repo.Setup(r => r.GetByIdAsync(9, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);
        var service = new CategoryService(repo.Object);

        var result = await service.AddToUserAsync(7, 9, CancellationToken.None);

        Assert.Equal(AddToUserResult.NotFound, result);
        repo.Verify(r => r.AddToUserAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddToUserAsync_Delegates_WhenCategoryExists()
    {
        var repo = new Mock<ICategoryRepository>();
        repo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category { IdCategory = 2, Name = "Salon" });
        repo.Setup(r => r.AddToUserAsync(7, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(AddToUserResult.Added);
        var service = new CategoryService(repo.Object);

        var result = await service.AddToUserAsync(7, 2, CancellationToken.None);

        Assert.Equal(AddToUserResult.Added, result);
    }

    [Fact]
    public async Task GetFavoriteCategoriesAsync_DelegatesToRepository()
    {
        var repo = new Mock<ICategoryRepository>();
        repo.Setup(r => r.GetFavoriteCategoriesAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category> { new() { IdCategory = 1, Name = "Conference" } });
        var service = new CategoryService(repo.Object);

        var categories = await service.GetFavoriteCategoriesAsync(7, CancellationToken.None);

        Assert.Single(categories);
        repo.Verify(r => r.GetFavoriteCategoriesAsync(7, It.IsAny<CancellationToken>()), Times.Once);
    }
}
