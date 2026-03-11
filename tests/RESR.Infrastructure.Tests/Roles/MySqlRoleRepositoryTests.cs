using System.Data;
using RESR.Core.Controllers.Permissions.Factories;
using RESR.Core.Controllers.Roles.Factories;
using RESR.Infrastructure.Roles;
using RESR.Infrastructure.Tests.DbFakes;

namespace RESR.Infrastructure.Tests.Roles;

public sealed class MySqlRoleRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsRoles()
    {
        var table = CreateRoleTable(Row(1, "User", null), Row(2, "Admin", "All"));
        var cmd = ReaderCommand(table);
        var repo = CreateRepo(cmd);

        var roles = await repo.GetAllAsync(CancellationToken.None);

        Assert.Equal(2, roles.Count);
        Assert.Equal("User", roles[0].Name);
        Assert.Equal("Admin", roles[1].Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsRole_WhenFound()
    {
        var table = CreateRoleTable(Row(3, "Mod", "Moderate"));
        var cmd = ReaderCommand(table);
        var repo = CreateRepo(cmd);

        var role = await repo.GetByIdAsync(3, CancellationToken.None);

        Assert.NotNull(role);
        Assert.Equal(3, role!.IdRole);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenMissing()
    {
        var cmd = ReaderCommand(CreateRoleTable());
        var repo = CreateRepo(cmd);

        var role = await repo.GetByIdAsync(99, CancellationToken.None);

        Assert.Null(role);
    }

    [Fact]
    public async Task GetPermissionsByRoleIdAsync_ReturnsPermissions()
    {
        var table = CreatePermissionTable(
            PermRow(1, "Read", null),
            PermRow(2, "Write", "Desc")
        );
        var cmd = ReaderCommand(table);
        var repo = CreateRepo(cmd);

        var permissions = await repo.GetPermissionsByRoleIdAsync(1, CancellationToken.None);

        Assert.Equal(2, permissions.Count);
        Assert.Equal("Read", permissions[0].Name);
        Assert.Equal("Write", permissions[1].Name);
    }

    [Fact]
    public async Task AddPermissionToRoleAsync_ReturnsTrue_WhenInserted()
    {
        var cmd = NonQueryCommand(1);
        var repo = CreateRepo(cmd);

        var added = await repo.AddPermissionToRoleAsync(1, 2, CancellationToken.None);

        Assert.True(added);
    }

    [Fact]
    public async Task AddPermissionToRoleAsync_ReturnsFalse_WhenIgnored()
    {
        var cmd = NonQueryCommand(0);
        var repo = CreateRepo(cmd);

        var added = await repo.AddPermissionToRoleAsync(1, 2, CancellationToken.None);

        Assert.False(added);
    }

    [Fact]
    public async Task RemovePermissionFromRoleAsync_ReturnsTrue_WhenDeleted()
    {
        var cmd = NonQueryCommand(1);
        var repo = CreateRepo(cmd);

        var removed = await repo.RemovePermissionFromRoleAsync(1, 2, CancellationToken.None);

        Assert.True(removed);
    }

    [Fact]
    public async Task RemovePermissionFromRoleAsync_ReturnsFalse_WhenMissing()
    {
        var cmd = NonQueryCommand(0);
        var repo = CreateRepo(cmd);

        var removed = await repo.RemovePermissionFromRoleAsync(1, 2, CancellationToken.None);

        Assert.False(removed);
    }

    private static MySqlRoleRepository CreateRepo(params FakeDbCommand[] commands)
    {
        return new MySqlRoleRepository(() => new FakeDbConnection(commands), new RoleFactory(), new PermissionFactory());
    }

    private static FakeDbCommand ReaderCommand(DataTable table)
    {
        return new FakeDbCommand
        {
            ExecuteReaderHandler = _ => table.CreateDataReader()
        };
    }

    private static FakeDbCommand NonQueryCommand(int rows)
    {
        return new FakeDbCommand
        {
            ExecuteNonQueryHandler = _ => rows
        };
    }

    private static DataTable CreateRoleTable(params object?[][] rows)
    {
        var table = new DataTable();
        table.Columns.Add("id_role", typeof(int));
        table.Columns.Add("name", typeof(string));
        table.Columns.Add("description", typeof(string));

        foreach (var row in rows)
            table.Rows.Add(row.Select(value => value ?? DBNull.Value).ToArray());

        return table;
    }

    private static DataTable CreatePermissionTable(params object?[][] rows)
    {
        var table = new DataTable();
        table.Columns.Add("id_permission", typeof(int));
        table.Columns.Add("name", typeof(string));
        table.Columns.Add("description", typeof(string));

        foreach (var row in rows)
            table.Rows.Add(row.Select(value => value ?? DBNull.Value).ToArray());

        return table;
    }

    private static object?[] Row(int id, string name, string? description) => new object?[]
    {
        id,
        name,
        description
    };

    private static object?[] PermRow(int id, string name, string? description) => new object?[]
    {
        id,
        name,
        description
    };
}
