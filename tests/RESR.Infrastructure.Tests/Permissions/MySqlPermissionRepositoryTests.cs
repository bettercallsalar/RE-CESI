using System.Data;
using RESR.Core.Controllers.Permissions.Factories;
using RESR.Infrastructure.Permissions;
using RESR.Infrastructure.Tests.DbFakes;
using RESR.Models.Permissions;

namespace RESR.Infrastructure.Tests.Permissions;

public sealed class MySqlPermissionRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsPermissions()
    {
        var table = CreatePermissionTable(
            Row(1, "Read", "Desc"),
            Row(2, "Write", null)
        );
        var cmd = ReaderCommand(table);
        var repo = CreateRepo(cmd);

        var list = await repo.GetAllAsync(CancellationToken.None);

        Assert.Equal(2, list.Count);
        Assert.Equal("Read", list[0].Name);
        Assert.Equal("Write", list[1].Name);
        Assert.Equal(string.Empty, list[1].Description);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsPermission_WhenFound()
    {
        var table = CreatePermissionTable(Row(5, "Admin", "All"));
        var cmd = ReaderCommand(table);
        var repo = CreateRepo(cmd);

        var permission = await repo.GetByIdAsync(5, CancellationToken.None);

        Assert.NotNull(permission);
        Assert.Equal(5, permission!.IdPermission);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenMissing()
    {
        var cmd = ReaderCommand(CreatePermissionTable());
        var repo = CreateRepo(cmd);

        var permission = await repo.GetByIdAsync(99, CancellationToken.None);

        Assert.Null(permission);
    }

    private static MySqlPermissionRepository CreateRepo(params FakeDbCommand[] commands)
    {
        return new MySqlPermissionRepository(() => new FakeDbConnection(commands), new PermissionFactory());
    }

    private static FakeDbCommand ReaderCommand(DataTable table)
    {
        return new FakeDbCommand
        {
            ExecuteReaderHandler = _ => table.CreateDataReader()
        };
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
}
