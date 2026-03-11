using Microsoft.AspNetCore.Mvc;
using Moq;
using RESR.Core.Controllers.Departments;
using RESR.Models.Departments;
using RESR.WebAPI.Routes.Departments;

namespace RESR.WebAPI.Tests.Departments;

public sealed class DepartmentsControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsOk_WithResponses()
    {
        var service = new Mock<IDepartmentService>();
        service.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Department>
            {
                new() { IdDepartment = 1, Name = "IT", Code = 10 },
                new() { IdDepartment = 2, Name = "HR", Code = 20 }
            });

        var controller = new DepartmentsController(service.Object);

        var result = await controller.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsType<List<DepartmentResponse>>(ok.Value);
        Assert.Collection(list,
            first => Assert.Equal(1, first.IdDepartment),
            second => Assert.Equal(2, second.IdDepartment));
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        var service = new Mock<IDepartmentService>();
        service.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Department?)null);
        var controller = new DepartmentsController(service.Object);

        var result = await controller.GetById(1, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenFound()
    {
        var service = new Mock<IDepartmentService>();
        service.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Department { IdDepartment = 1, Name = "IT", Code = 10 });
        var controller = new DepartmentsController(service.Object);

        var result = await controller.GetById(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<DepartmentResponse>(ok.Value);
        Assert.Equal(1, response.IdDepartment);
    }
}
