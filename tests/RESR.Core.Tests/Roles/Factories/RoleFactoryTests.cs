using RESR.Core.Controllers.Roles.Factories;

namespace RESR.Core.Tests.Roles.Factories;

public sealed class RoleFactoryTests
{
    [Fact]
    public void Create_AssignsFields()
    {
        var factory = new RoleFactory();

        var role = factory.Create(2, "Admin", "All");

        Assert.Equal(2, role.IdRole);
        Assert.Equal("Admin", role.Name);
        Assert.Equal("All", role.Description);
    }
}
