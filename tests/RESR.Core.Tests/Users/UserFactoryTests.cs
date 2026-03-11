using RESR.Core.Controllers.Users.Factories;
using RESR.Models.Users;

namespace RESR.Core.Tests.Users;

public sealed class UserFactoryTests
{
    [Fact]
    public void CreateForRegistration_BuildsNewUserWithDefaults()
    {
        var factory = new UserFactory();
        var birthDate = new DateOnly(1999, 12, 31);

        var user = factory.CreateForRegistration(
            "user",
            "user@example.com",
            "hash",
            "User",
            birthDate,
            "bio",
            1,
            2
        );

        Assert.Equal("user", user.Username);
        Assert.Equal("user@example.com", user.Email);
        Assert.Equal("hash", user.HashedPassword);
        Assert.Equal("User", user.FirstName);
        Assert.Equal(birthDate, user.BirthDate);
        Assert.Equal("bio", user.Bio);
        Assert.Equal(1, user.IdDepartment);
        Assert.Equal(2, user.IdRole);
        Assert.False(user.IsVerified);
        Assert.Null(user.DeletedAt);
    }

    [Fact]
    public void CreateFromPersistence_MapsAllFields()
    {
        var factory = new UserFactory();
        var deletedAt = DateTime.UtcNow;

        var user = factory.CreateFromPersistence(
            10,
            "user",
            "user@example.com",
            "hash",
            "User",
            new DateOnly(2000, 1, 1),
            null,
            true,
            deletedAt,
            3,
            4
        );

        Assert.Equal(10, user.IdUser);
        Assert.Equal("user", user.Username);
        Assert.Equal("user@example.com", user.Email);
        Assert.Equal("hash", user.HashedPassword);
        Assert.Equal("User", user.FirstName);
        Assert.True(user.IsVerified);
        Assert.Equal(deletedAt, user.DeletedAt);
        Assert.Equal(3, user.IdDepartment);
        Assert.Equal(4, user.IdRole);
    }
}
