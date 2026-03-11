using Microsoft.AspNetCore.Mvc;
using Moq;
using RESR.Core.Controllers.Categories;
using RESR.Models.Categories;
using RESR.WebAPI.Routes.Categories;

namespace RESR.WebAPI.Tests.Categories;

public sealed class CategoriesControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsOk_WithList()
    {
        var service = new Mock<ICategoryService>();
        service.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category> { new() { IdCategory = 1, Name = "Atelier" } });
        var controller = new CategoriesController(service.Object);

        var result = await controller.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsType<List<CategoryResponse>>(ok.Value);
        Assert.Single(list);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        var service = new Mock<ICategoryService>();
        service.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);
        var controller = new CategoriesController(service.Object);

        var result = await controller.GetById(1, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenFound()
    {
        var service = new Mock<ICategoryService>();
        service.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category { IdCategory = 1, Name = "Atelier" });
        var controller = new CategoriesController(service.Object);

        var result = await controller.GetById(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<CategoryResponse>(ok.Value);
        Assert.Equal(1, response.IdCategory);
    }
}
