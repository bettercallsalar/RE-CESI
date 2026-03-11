using RESR.Models.Departments;
using RESR.Models.Permissions;
using RESR.Models.Roles;
using RESR.Models.Users;
using RESR.Models.Categories;

namespace RESR.Models.Tests;

public sealed class ModelsTests
{
    [Fact]
    public void User_AllowsPropertyAssignment()
    {
        var user = new User
        {
            IdUser = 1,
            Username = "user",
            Email = "user@example.com",
            FirstName = "User",
            HashedPassword = "hash",
            BirthDate = new DateOnly(2000, 1, 1),
            Bio = "bio",
            IsVerified = true,
            DeletedAt = DateTime.UtcNow,
            IdDepartment = 1,
            IdRole = 2
        };

        Assert.Equal(1, user.IdUser);
        Assert.Equal("user", user.Username);
        Assert.Equal("user@example.com", user.Email);
        Assert.Equal("User", user.FirstName);
        Assert.Equal("hash", user.HashedPassword);
        Assert.Equal(new DateOnly(2000, 1, 1), user.BirthDate);
        Assert.Equal("bio", user.Bio);
        Assert.True(user.IsVerified);
        Assert.NotNull(user.DeletedAt);
        Assert.Equal(1, user.IdDepartment);
        Assert.Equal(2, user.IdRole);
    }

    [Fact]
    public void Role_AllowsPropertyAssignment()
    {
        var role = new Role
        {
            IdRole = 1,
            Name = "Admin",
            Description = "All"
        };

        Assert.Equal(1, role.IdRole);
        Assert.Equal("Admin", role.Name);
        Assert.Equal("All", role.Description);
    }

    [Fact]
    public void Permission_AllowsPropertyAssignment()
    {
        var permission = new Permission
        {
            IdPermission = 1,
            Name = "Read",
            Description = "Desc"
        };

        Assert.Equal(1, permission.IdPermission);
        Assert.Equal("Read", permission.Name);
        Assert.Equal("Desc", permission.Description);
    }

    [Fact]
    public void Department_AllowsPropertyAssignment()
    {
        var department = new Department
        {
            IdDepartment = 42,
            Name = "Engineering",
            Code = 101
        };

        Assert.Equal(42, department.IdDepartment);
        Assert.Equal("Engineering", department.Name);
        Assert.Equal(101, department.Code);
    }

    [Fact]
    public void Category_AllowsPropertyAssignment()
    {
        var category = new Category
        {
            IdCategory = 1,
            Name = "Atelier"
        };

        Assert.Equal(1, category.IdCategory);
        Assert.Equal("Atelier", category.Name);
    }

    [Fact]
    public void UserDtos_AssignValues()
    {
        var register = new RegisterUserRequest("u", "e", "p", "f", null, null, 1);
        var update = new UpdateUserRequest("u", "e", "f", null, "b", 1, 2);
        var updateOwn = new UpdateOwnProfileRequest("u", "e", "f", null, "b", 1);
        var verification = new SetUserVerificationRequest(true);
        var response = new UserResponse(1, "u", "e", "f", null, "b", true, 1, 2);
        var paginated = new PaginatedUsersResponse(new List<UserResponse> { response }, 1, 10, 1, 1);
        var filters = new UserListingFilters("k", new List<int> { 1 }, new List<int> { 2 }, null, true, false);
        var login = new Login("e", "p");

        Assert.Equal("u", register.Username);
        Assert.Equal("e", update.Email);
        Assert.Equal("f", updateOwn.FirstName);
        Assert.True(verification.IsVerified);
        Assert.Equal(1, response.IdUser);
        Assert.Single(paginated.Items);
        Assert.Equal("k", filters.Keyword);
        Assert.Equal("e", login.Email);
    }

    [Fact]
    public void PermissionAndRoleDtos_AssignValues()
    {
        var permission = new PermissionResponse(1, "P", "D");
        var role = new RoleResponse(1, "R", "Desc", new List<PermissionResponse> { permission });

        Assert.Equal(1, permission.IdPermission);
        Assert.Equal("P", permission.Name);
        Assert.Equal("R", role.Name);
        Assert.Single(role.Permissions);
    }

    [Fact]
    public void DepartmentResponse_ExposesAllProperties()
    {
        var response = new DepartmentResponse(7, "Support", 501);

        Assert.Equal(7, response.IdDepartment);
        Assert.Equal("Support", response.Name);
        Assert.Equal(501, response.Code);
    }

    [Fact]
    public void CategoryDto_AssignsValues()
    {
        var response = new CategoryResponse(5, "Conference");

        Assert.Equal(5, response.IdCategory);
        Assert.Equal("Conference", response.Name);
    }

    [Fact]
    public void UserResponse_ExposesAllProperties()
    {
        var response = new UserResponse(10, "user", "user@example.com", "User", new DateOnly(1999, 12, 31), "bio", false, 3, 4);

        Assert.Equal(10, response.IdUser);
        Assert.Equal("user", response.Username);
        Assert.Equal("user@example.com", response.Email);
        Assert.Equal("User", response.FirstName);
        Assert.Equal(new DateOnly(1999, 12, 31), response.BirthDate);
        Assert.Equal("bio", response.Bio);
        Assert.False(response.IsVerified);
        Assert.Equal(3, response.IdDepartment);
        Assert.Equal(4, response.IdRole);
    }

    [Fact]
    public void UpdateUserRequest_ExposesAllProperties()
    {
        var request = new UpdateUserRequest("u", "e", "f", new DateOnly(2001, 1, 1), "bio", 2, 3);

        Assert.Equal("u", request.Username);
        Assert.Equal("e", request.Email);
        Assert.Equal("f", request.FirstName);
        Assert.Equal(new DateOnly(2001, 1, 1), request.BirthDate);
        Assert.Equal("bio", request.Bio);
        Assert.Equal(2, request.IdDepartment);
        Assert.Equal(3, request.IdRole);
    }

    [Fact]
    public void PermissionAndRoleResponses_ExposeDescriptions()
    {
        var permission = new PermissionResponse(2, "Perm", "Desc");
        var role = new RoleResponse(2, "Role", "RoleDesc", new List<PermissionResponse> { permission });

        Assert.Equal("Desc", permission.Description);
        Assert.Equal("RoleDesc", role.Description);
    }
}
