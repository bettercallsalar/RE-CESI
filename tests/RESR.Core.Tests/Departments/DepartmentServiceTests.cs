using Moq;
using RESR.Core.Controllers.Departments;
using RESR.Core.Controllers.Departments.Ports;
using RESR.Models.Departments;

namespace RESR.Core.Tests.Departments;

public sealed class DepartmentServiceTests
{
    [Fact]
    public async Task GetAllAsync_DelegatesToRepository()
    {
        var repo = new Mock<IDepartmentRepository>();
        var expected = new List<Department> { new() { IdDepartment = 1, Name = "IT", Code = 10 } };
        repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var service = new DepartmentService(repo.Object);

        var result = await service.GetAllAsync(CancellationToken.None);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task GetByIdAsync_DelegatesToRepository()
    {
        var repo = new Mock<IDepartmentRepository>();
        var expected = new Department { IdDepartment = 5, Name = "HR", Code = 20 };
        repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var service = new DepartmentService(repo.Object);

        var result = await service.GetByIdAsync(5, CancellationToken.None);

        Assert.Same(expected, result);
    }
}
