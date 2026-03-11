using RESR.Core.Controllers.Permissions.Factories;

namespace RESR.Core.Tests.Permissions.Factories;

public sealed class PermissionFactoryTests
{
    [Fact]
    public void Create_AssignsFields_AndDefaultsDescription()
    {
        var factory = new PermissionFactory();

        var permission = factory.Create(1, "Read", null);

        Assert.Equal(1, permission.IdPermission);
        Assert.Equal("Read", permission.Name);
        Assert.Equal(string.Empty, permission.Description);
    }
}
