using Microsoft.AspNetCore.Http;
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

    [Fact]
    public async Task AddToUser_ReturnsNoContent_WhenAdded()
    {
        var service = new Mock<ICategoryService>();
        service.Setup(s => s.AddToUserAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(AddToUserResult.Added);
        var controller = new CategoriesController(service.Object);

        var result = await controller.AddToUser(2, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task AddToUser_ReturnsConflict_WhenAlreadyExists()
    {
        var service = new Mock<ICategoryService>();
        service.Setup(s => s.AddToUserAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(AddToUserResult.AlreadyExists);
        var controller = new CategoriesController(service.Object);

        var result = await controller.AddToUser(2, CancellationToken.None);

        Assert.IsType<ConflictResult>(result);
    }

    [Fact]
    public async Task AddToUser_ReturnsNotFound_WhenMissing()
    {
        var service = new Mock<ICategoryService>();
        service.Setup(s => s.AddToUserAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(AddToUserResult.NotFound);
        var controller = new CategoriesController(service.Object);

        var result = await controller.AddToUser(2, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AddToUser_ReturnsServerError_WhenUnexpectedResult()
    {
        var service = new Mock<ICategoryService>();
        service.Setup(s => s.AddToUserAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AddToUserResult)999);
        var controller = new CategoriesController(service.Object);

        var result = await controller.AddToUser(2, CancellationToken.None);

        var status = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, status.StatusCode);
    }

    [Fact]
    public async Task RemoveFromUser_ReturnsNoContent_WhenSuccess()
    {
        var service = new Mock<ICategoryService>();
        service.Setup(s => s.RemoveFromUserAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var controller = new CategoriesController(service.Object);

        var result = await controller.RemoveFromUser(2, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task RemoveFromUser_ReturnsNotFound_WhenMissing()
    {
        var service = new Mock<ICategoryService>();
        service.Setup(s => s.RemoveFromUserAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var controller = new CategoriesController(service.Object);

        var result = await controller.RemoveFromUser(2, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }
}
