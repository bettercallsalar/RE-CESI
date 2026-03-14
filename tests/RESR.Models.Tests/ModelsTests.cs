using RESR.Models.Departments;
using RESR.Models.Follows;
using RESR.Models.Comments;
using RESR.Models.Permissions;
using RESR.Models.Reactions;
using RESR.Models.Roles;
using RESR.Models.Users;
using RESR.Models.Categories;
using RESR.Models.Marks;

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
            IsBanned = true,
            DeletedAt = DateTime.UtcNow,
            Department = new Department { IdDepartment = 1, Name = "IT", Code = 10 },
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
        Assert.True(user.IsBanned);
        Assert.NotNull(user.DeletedAt);
        Assert.Equal(1, user.Department.IdDepartment);
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
    public void Follow_AllowsPropertyAssignment()
    {
        var follow = new Follow
        {
            IdFollower = 3,
            IdFollowing = 9
        };

        Assert.Equal(3, follow.IdFollower);
        Assert.Equal(9, follow.IdFollowing);
    }

    [Fact]
    public void FollowUser_AllowsPropertyAssignment()
    {
        var followUser = new FollowUser
        {
            IdUser = 5,
            Username = "user",
            FirstName = "User"
        };

        Assert.Equal(5, followUser.IdUser);
        Assert.Equal("user", followUser.Username);
        Assert.Equal("User", followUser.FirstName);
    }

    [Fact]
    public void Comment_AllowsPropertyAssignment()
    {
        var comment = new Comment
        {
            IdComment = 5,
            Content = "Hello",
            CreatedAt = new DateTime(2026, 3, 11, 12, 0, 0, DateTimeKind.Utc),
            ModifiedAt = new DateTime(2026, 3, 11, 13, 0, 0, DateTimeKind.Utc),
            DeletedAt = null,
            IdResource = 9,
            IdUser = 3,
            IdParentComment = 1
        };

        Assert.Equal(5, comment.IdComment);
        Assert.Equal("Hello", comment.Content);
        Assert.Equal(9, comment.IdResource);
        Assert.Equal(3, comment.IdUser);
        Assert.Equal(1, comment.IdParentComment);
    }

    [Fact]
    public void Reaction_AllowsPropertyAssignment()
    {
        var reaction = new Reaction
        {
            IdReaction = 3,
            Name = ReactionNames.Love,
            IdResource = 9,
            IdUser = 4,
            Username = "user4",
            FirstName = "User Four"
        };

        Assert.Equal(3, reaction.IdReaction);
        Assert.Equal(ReactionNames.Love, reaction.Name);
        Assert.Equal(9, reaction.IdResource);
        Assert.Equal(4, reaction.IdUser);
        Assert.Equal("user4", reaction.Username);
        Assert.Equal("User Four", reaction.FirstName);
    }

    [Fact]
    public void UserDtos_AssignValues()
    {
        var register = new RegisterUserRequest("u", "e", "p", "f", null, null, 1);
        var update = new UpdateUserRequest("u", "e", "f", null, "b", 1, 2);
        var updateOwn = new UpdateOwnProfileRequest("u", "e", "f", null, "b", 1);
        var verification = new SetUserVerificationRequest(true);
        var ban = new SetUserBanRequest(true);
        var response = new UserResponse(1, "u", "e", "f", null, "b", true, false, new Department { IdDepartment = 1, Name = "IT", Code = 10 }, 2);
        var paginated = new PaginatedUsersResponse(new List<UserResponse> { response }, 1, 10, 1, 1);
        var filters = new UserListingFilters("k", new List<int> { 1 }, new List<int> { 2 }, null, true, false);
        var login = new Login("e", "p");

        Assert.Equal("u", register.Username);
        Assert.Equal("e", update.Email);
        Assert.Equal("f", updateOwn.FirstName);
        Assert.True(verification.IsVerified);
        Assert.True(ban.IsBanned);
        Assert.Equal(1, response.IdUser);
        Assert.Single(paginated.Items);
        Assert.Equal("k", filters.Keyword);
        Assert.Equal("e", login.Email);
    }

    [Fact]
    public void FollowDtos_AssignValues()
    {
        var request = new FollowRequest(1, 2);
        var response = new FollowResponse(1, 2);
        var user = new FollowUserResponse(3, "user", "User");
        var paginated = new PaginatedFollowUsersResponse(new List<FollowUserResponse> { user }, 1, 10, 1, 1);

        Assert.Equal(1, request.IdFollower);
        Assert.Equal(2, response.IdFollowing);
        Assert.Equal("user", user.Username);
        Assert.Single(paginated.Items);
        Assert.Equal(1, paginated.TotalCount);
    }

    [Fact]
    public void CommentDtos_AssignValues()
    {
        var create = new CreateCommentRequest("Hello", 1);
        var update = new UpdateCommentRequest("Updated");
        var response = new CommentResponse(
            7,
            "Hello",
            new DateTime(2026, 3, 11, 12, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 11, 13, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 11, 14, 0, 0, DateTimeKind.Utc),
            10,
            2,
            1
        );

        Assert.Equal("Hello", create.Content);
        Assert.Equal(1, create.IdParentComment);
        Assert.Equal("Updated", update.Content);
        Assert.Equal(7, response.IdComment);
        Assert.Equal(new DateTime(2026, 3, 11, 14, 0, 0, DateTimeKind.Utc), response.DeletedAt);
        Assert.Equal(10, response.IdResource);
        Assert.Equal(2, response.IdUser);
        Assert.Equal(1, response.IdParentComment);
    }

    [Fact]
    public void ReactionDtos_AssignValues()
    {
        var create = new CreateReactionRequest(ReactionNames.Like);
        var update = new UpdateReactionRequest(ReactionNames.Dislike);
        var user = new ReactionUserResponse(2, "user2", "User Two");
        var response = new ReactionResponse(8, ReactionNames.Love, 11, 2, user);
        var userReactions = new UserReactionsResponse(2, 1, new List<ReactionResponse> { response });

        Assert.Equal(ReactionNames.Like, create.Name);
        Assert.Equal(ReactionNames.Dislike, update.Name);
        Assert.Equal(8, response.IdReaction);
        Assert.Equal(11, response.IdResource);
        Assert.Equal(2, response.IdUser);
        Assert.Equal("user2", response.User.Username);
        Assert.Equal(2, userReactions.IdUser);
        Assert.Equal(1, userReactions.TotalCount);
        Assert.Single(userReactions.Items);
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
        var response = new UserResponse(10, "user", "user@example.com", "User", new DateOnly(1999, 12, 31), "bio", false, true, new Department { IdDepartment = 3, Name = "HR", Code = 20 }, 4);

        Assert.Equal(10, response.IdUser);
        Assert.Equal("user", response.Username);
        Assert.Equal("user@example.com", response.Email);
        Assert.Equal("User", response.FirstName);
        Assert.Equal(new DateOnly(1999, 12, 31), response.BirthDate);
        Assert.Equal("bio", response.Bio);
        Assert.False(response.IsVerified);
        Assert.True(response.IsBanned);
        Assert.Equal(3, response.Department.IdDepartment);
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

    [Fact]
    public void MarkDtos_AssignValues()
    {
        var create = new CreateMarkRequest(true, false, 4);
        var update = new UpdateMarkRequest(false, true, 4);
        var response = new MarkResponse(7, true, false, 4, 2);
        var paginated = new PaginatedMarksResponse(new List<MarkResponse> { response }, 1, 10, 1, 1);
        var mark = new Mark
        {
            IdMark = 7,
            IsFavorite = true,
            IsReadLater = false,
            IdRessource = 4,
            IdUser = 2
        };

        Assert.True(create.IsFavorite);
        Assert.True(update.IsReadLater);
        Assert.Equal(7, response.IdMark);
        Assert.Single(paginated.Items);
        Assert.Equal(4, mark.IdRessource);
    }
}
