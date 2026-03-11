using RESR.Core.Controllers.Departments.Factories;

namespace RESR.Core.Tests.Departments;

public sealed class DepartmentFactoryTests
{
    [Fact]
    public void Create_MapsAllFields()
    {
        var factory = new DepartmentFactory();

        var department = factory.Create(4, "Finance", 120);

        Assert.Equal(4, department.IdDepartment);
        Assert.Equal("Finance", department.Name);
        Assert.Equal(120, department.Code);
    }
}
