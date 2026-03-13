using System.Data;
using System.Data.Common;
using RESR.Core.Controllers.Users;
using RESR.Core.Controllers.Users.Factories;
using RESR.Infrastructure.Tests.DbFakes;
using RESR.Infrastructure.Users;
using RESR.Models.Departments;
using RESR.Models.Users;

namespace RESR.Infrastructure.Tests.Users;

public sealed class MySqlUserRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_ReturnsUser_WhenExists()
    {
        var table = CreateUserTable(Row(
            10,
            "alice",
            "Alice",
            new DateTime(1990, 1, 2),
            "bio",
            "alice@example.com",
            "hash",
            true,
            null,
            1,
            2
        ));

        var cmd = ReaderCommand(table);
        var repo = CreateRepo(cmd);

        var user = await repo.GetByIdAsync(10, CancellationToken.None);

        Assert.NotNull(user);
        Assert.Equal(10, user!.IdUser);
        Assert.Equal("alice", user.Username);
        Assert.Equal("Alice", user.FirstName);
        Assert.Equal(new DateOnly(1990, 1, 2), user.BirthDate);
        Assert.Equal("bio", user.Bio);
        Assert.Equal("alice@example.com", user.Email);
        Assert.Equal("hash", user.HashedPassword);
        Assert.True(user.IsVerified);
        Assert.Null(user.DeletedAt);
        Assert.Equal(1, user.Department.IdDepartment);
        Assert.Equal("Department 1", user.Department.Name);
        Assert.Equal(2, user.IdRole);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNoRow()
    {
        var cmd = ReaderCommand(CreateUserTable());
        var repo = CreateRepo(cmd);

        var user = await repo.GetByIdAsync(10, CancellationToken.None);

        Assert.Null(user);
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsUser_WhenExists()
    {
        var table = CreateUserTable(Row(1, "bob", "Bob", null, null, "bob@example.com", "hash", false, null, 1, 2));
        var cmd = ReaderCommand(table);
        var repo = CreateRepo(cmd);

        var user = await repo.GetByEmailAsync("bob@example.com", CancellationToken.None);

        Assert.NotNull(user);
        Assert.Equal("bob", user!.Username);
        Assert.Null(user.BirthDate);
    }

    [Fact]
    public async Task GetByUsernameAsync_ReturnsUser_WhenExists()
    {
        var table = CreateUserTable(Row(2, "carol", "Carol", null, null, "carol@example.com", "hash", false, null, 1, 2));
        var cmd = ReaderCommand(table);
        var repo = CreateRepo(cmd);

        var user = await repo.GetByUsernameAsync("carol", CancellationToken.None);

        Assert.NotNull(user);
        Assert.Equal("carol@example.com", user!.Email);
    }

    [Fact]
    public async Task GetByEmailAndPasswordHashAsync_ReturnsUser_WhenExists()
    {
        var table = CreateUserTable(Row(3, "dave", "Dave", null, null, "dave@example.com", "hash", true, null, 1, 2));
        var cmd = ReaderCommand(table);
        var repo = CreateRepo(cmd);

        var user = await repo.GetByEmailAndPasswordHashAsync("dave@example.com", "hash", CancellationToken.None);

        Assert.NotNull(user);
        Assert.Equal(3, user!.IdUser);
    }

    [Fact]
    public async Task GetUsersPaginatedAsync_AppliesFilters_AndReturnsRows()
    {
        var table = CreateUserTable(Row(5, "eve", "Eve", null, null, "eve@example.com", "hash", true, null, 2, 3));
        var cmd = ReaderCommand(table);
        var repo = CreateRepo(cmd);
        var filters = new UserListingFilters(
            Keyword: "eve",
            DepartmentIds: new List<int> { 2, 4 },
            RoleIds: new List<int> { 3 },
            BirthDate: new DateOnly(2000, 2, 2),
            IsVerified: true,
            IncludeDeleted: false
        );

        var users = await repo.GetUsersPaginatedAsync(2, 10, filters, CancellationToken.None);

        Assert.Single(users);
        Assert.Equal("eve", users[0].Username);
        Assert.Contains("deleted_at IS NULL", cmd.CommandText, StringComparison.OrdinalIgnoreCase);
        var names = cmd.Parameters.Cast<DbParameter>().Select(p => p.ParameterName).ToList();
        Assert.Contains("@keyword", names);
        Assert.Contains("@birth_date", names);
        Assert.Contains("@is_verified", names);
        Assert.Contains("@dep0", names);
        Assert.Contains("@role0", names);
    }

    [Fact]
    public async Task GetUsersPaginatedAsync_AllowsIncludeDeleted_AndSkipsEmptyFilters()
    {
        var table = CreateUserTable(Row(6, "frank", "Frank", null, null, "frank@example.com", "hash", false, DateTime.UtcNow, 1, 1));
        var cmd = ReaderCommand(table);
        var repo = CreateRepo(cmd);
        var filters = new UserListingFilters(
            Keyword: null,
            DepartmentIds: Array.Empty<int>(),
            RoleIds: null,
            BirthDate: null,
            IsVerified: null,
            IncludeDeleted: true
        );

        var users = await repo.GetUsersPaginatedAsync(1, 10, filters, CancellationToken.None);

        Assert.Single(users);
        Assert.DoesNotContain("deleted_at IS NULL", cmd.CommandText, StringComparison.OrdinalIgnoreCase);
        var names = cmd.Parameters.Cast<DbParameter>().Select(p => p.ParameterName).ToList();
        Assert.Equal(new[] { "@limit", "@offset" }, names);
    }

    [Fact]
    public async Task CountUsersAsync_ReturnsCount()
    {
        var cmd = ScalarCommand(7);
        var repo = CreateRepo(cmd);
        var filters = new UserListingFilters("k", new List<int> { 1 }, null, null, null, false);

        var count = await repo.CountUsersAsync(filters, CancellationToken.None);

        Assert.Equal(7, count);
        Assert.Contains("deleted_at IS NULL", cmd.CommandText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_InsertsAndReturnsId()
    {
        var cmd = ScalarCommand(42);
        var repo = CreateRepo(cmd);

        var user = new User
        {
            Username = "new",
            Email = "new@example.com",
            HashedPassword = "hash",
            FirstName = "New",
            BirthDate = new DateOnly(2001, 1, 1),
            Bio = null,
            Department = new Department { IdDepartment = 1, Name = "IT", Code = 10 },
            IdRole = 2,
            IsVerified = false,
            DeletedAt = null
        };

        var id = await repo.CreateAsync(user, CancellationToken.None);

        Assert.Equal(42, id);
        Assert.Contains("@username", cmd.Parameters.Cast<DbParameter>().Select(p => p.ParameterName));
    }

    [Fact]
    public async Task PatchAsync_UpdatesAndReturnsUser()
    {
        var updateCmd = NonQueryCommand(1);
        var selectCmd = ReaderCommand(CreateUserTable(Row(8, "hank", "Hank", null, null, "hank@example.com", "hash", false, null, 1, 2)));
        var repo = CreateRepo(updateCmd, selectCmd);

        var result = await repo.PatchAsync(new UpdateUserCommand(IdUser: 8, Username: "hank"), CancellationToken.None);

        Assert.Equal(8, result.IdUser);
        Assert.Equal("hank", result.Username);
        Assert.Contains("@id_user", updateCmd.Parameters.Cast<DbParameter>().Select(p => p.ParameterName));
    }

    [Fact]
    public async Task PatchAsync_Throws_WhenNoRowsUpdated()
    {
        var updateCmd = NonQueryCommand(0);
        var repo = CreateRepo(updateCmd);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.PatchAsync(new UpdateUserCommand(IdUser: 404), CancellationToken.None));
    }

    [Fact]
    public async Task SoftDeleteAsync_ReturnsTrue_WhenUpdated()
    {
        var cmd = NonQueryCommand(1);
        var repo = CreateRepo(cmd);

        var ok = await repo.SoftDeleteAsync(10, CancellationToken.None);

        Assert.True(ok);
    }

    [Fact]
    public async Task SoftDeleteAsync_ReturnsFalse_WhenNoRows()
    {
        var cmd = NonQueryCommand(0);
        var repo = CreateRepo(cmd);

        var ok = await repo.SoftDeleteAsync(10, CancellationToken.None);

        Assert.False(ok);
    }

    [Fact]
    public async Task SetVerificationAsync_UpdatesAndReturnsUser()
    {
        var updateCmd = NonQueryCommand(1);
        var selectCmd = ReaderCommand(CreateUserTable(Row(9, "ivy", "Ivy", null, null, "ivy@example.com", "hash", true, null, 1, 2)));
        var repo = CreateRepo(updateCmd, selectCmd);

        var user = await repo.SetVerificationAsync(9, true, CancellationToken.None);

        Assert.True(user.IsVerified);
    }

    [Fact]
    public async Task SetVerificationAsync_Throws_WhenNoRowsUpdated()
    {
        var updateCmd = NonQueryCommand(0);
        var repo = CreateRepo(updateCmd);

        await Assert.ThrowsAsync<InvalidOperationException>(() => repo.SetVerificationAsync(9, true, CancellationToken.None));
    }

    [Fact]
    public async Task Map_Throws_WhenFirstNameIsNull()
    {
        var table = CreateUserTable(Row(1, "u", null, null, null, "u@example.com", "hash", false, null, 1, 1));
        var cmd = ReaderCommand(table);
        var repo = CreateRepo(cmd);

        await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetByIdAsync(1, CancellationToken.None));
    }

    [Fact]
    public async Task Map_Throws_WhenDepartmentIsNull()
    {
        var table = CreateUserTable(Row(1, "u", "User", null, null, "u@example.com", "hash", false, null, null, 1));
        var cmd = ReaderCommand(table);
        var repo = CreateRepo(cmd);

        await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetByIdAsync(1, CancellationToken.None));
    }

    [Fact]
    public async Task Map_Throws_WhenRoleIsNull()
    {
        var table = CreateUserTable(Row(1, "u", "User", null, null, "u@example.com", "hash", false, null, 1, null));
        var cmd = ReaderCommand(table);
        var repo = CreateRepo(cmd);

        await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetByIdAsync(1, CancellationToken.None));
    }

    private static MySqlUserRepository CreateRepo(params FakeDbCommand[] commands)
    {
        DbConnection ConnectionFactory() => new FakeDbConnection(commands);
        return new MySqlUserRepository(ConnectionFactory, new UserFactory());
    }

    private static FakeDbCommand ReaderCommand(DataTable table)
    {
        return new FakeDbCommand
        {
            ExecuteReaderHandler = _ => table.CreateDataReader()
        };
    }

    private static FakeDbCommand ScalarCommand(object result)
    {
        return new FakeDbCommand
        {
            ExecuteScalarHandler = _ => result
        };
    }

    private static FakeDbCommand NonQueryCommand(int rows)
    {
        return new FakeDbCommand
        {
            ExecuteNonQueryHandler = _ => rows
        };
    }

    private static DataTable CreateUserTable(params object?[][] rows)
    {
        var table = new DataTable();
        table.Columns.Add("id_user", typeof(int));
        table.Columns.Add("username", typeof(string));
        table.Columns.Add("first_name", typeof(string));
        table.Columns.Add("birth_date", typeof(DateTime));
        table.Columns.Add("bio", typeof(string));
        table.Columns.Add("email", typeof(string));
        table.Columns.Add("hashed_password", typeof(string));
        table.Columns.Add("is_verified", typeof(bool));
        table.Columns.Add("deleted_at", typeof(DateTime));
        table.Columns.Add("id_department", typeof(int));
        table.Columns.Add("id_role", typeof(int));
        table.Columns.Add("department_name", typeof(string));
        table.Columns.Add("department_code", typeof(int));

        foreach (var row in rows)
            table.Rows.Add(row.Select(value => value ?? DBNull.Value).ToArray());

        return table;
    }

    private static object?[] Row(
        int idUser,
        string username,
        string? firstName,
        DateTime? birthDate,
        string? bio,
        string email,
        string hashedPassword,
        bool isVerified,
        DateTime? deletedAt,
        int? idDepartment,
        int? idRole
    ) => new object?[]
    {
        idUser,
        username,
        firstName,
        birthDate,
        bio,
        email,
        hashedPassword,
        isVerified,
        deletedAt,
        idDepartment,
        idRole,
        idDepartment is null ? null : $"Department {idDepartment}",
        idDepartment is null ? null : idDepartment * 10
    };
}
