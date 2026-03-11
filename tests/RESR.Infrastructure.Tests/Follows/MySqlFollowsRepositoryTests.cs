using System.Data;
using System.Data.Common;
using RESR.Core.Controllers.Follows.Factories;
using RESR.Infrastructure.Follows;
using RESR.Infrastructure.Tests.DbFakes;
using RESR.Models.Follows;

namespace RESR.Infrastructure.Tests.Follows;

public sealed class MySqlFollowsRepositoryTests
{
    [Fact]
    public async Task GetAllFollowersAsync_ReturnsUsers_WhenExists()
    {
        var table = CreateUserTable(Row(1, "alice", "Alice"));
        var cmd = ReaderCommand(table);
        var repo = CreateRepo(cmd);

        var users = await repo.GetAllFollowersAsync(5, CancellationToken.None);

        Assert.Single(users);
        Assert.Equal(1, users[0].IdUser);
        Assert.Equal("alice", users[0].Username);
        Assert.Contains("deleted_at IS NULL", cmd.CommandText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("@idUser", cmd.Parameters.Cast<DbParameter>().Select(p => p.ParameterName));
    }

    [Fact]
    public async Task GetAllFollowingAsync_ReturnsUsers_WhenExists()
    {
        var table = CreateUserTable(Row(2, "bob", "Bob"));
        var cmd = ReaderCommand(table);
        var repo = CreateRepo(cmd);

        var users = await repo.GetAllFollowingAsync(9, CancellationToken.None);

        Assert.Single(users);
        Assert.Equal(2, users[0].IdUser);
        Assert.Equal("Bob", users[0].FirstName);
        Assert.Contains("deleted_at IS NULL", cmd.CommandText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("@idUser", cmd.Parameters.Cast<DbParameter>().Select(p => p.ParameterName));
    }

    [Fact]
    public async Task CreateAsync_ReturnsTrue_WhenInserted()
    {
        var cmd = NonQueryCommand(1);
        var repo = CreateRepo(cmd);

        var ok = await repo.CreateAsync(3, 4, CancellationToken.None);

        Assert.True(ok);
        var names = cmd.Parameters.Cast<DbParameter>().Select(p => p.ParameterName).ToList();
        Assert.Contains("@idFollower", names);
        Assert.Contains("@idFollowing", names);
    }

    [Fact]
    public async Task CreateAsync_ReturnsFalse_WhenNoRows()
    {
        var cmd = NonQueryCommand(0);
        var repo = CreateRepo(cmd);

        var ok = await repo.CreateAsync(3, 4, CancellationToken.None);

        Assert.False(ok);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenDeleted()
    {
        var cmd = NonQueryCommand(1);
        var repo = CreateRepo(cmd);

        var ok = await repo.DeleteAsync(3, 4, CancellationToken.None);

        Assert.True(ok);
        var names = cmd.Parameters.Cast<DbParameter>().Select(p => p.ParameterName).ToList();
        Assert.Contains("@idFollower", names);
        Assert.Contains("@idFollowing", names);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNoRows()
    {
        var cmd = NonQueryCommand(0);
        var repo = CreateRepo(cmd);

        var ok = await repo.DeleteAsync(3, 4, CancellationToken.None);

        Assert.False(ok);
    }

    private static MySqlFollowsRepository CreateRepo(params FakeDbCommand[] commands)
    {
        DbConnection ConnectionFactory() => new FakeDbConnection(commands);
        return new MySqlFollowsRepository(ConnectionFactory, new FollowsFactory());
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

    private static DataTable CreateUserTable(params object?[][] rows)
    {
        var table = new DataTable();
        table.Columns.Add("id_user", typeof(int));
        table.Columns.Add("username", typeof(string));
        table.Columns.Add("first_name", typeof(string));

        foreach (var row in rows)
            table.Rows.Add(row.Select(value => value ?? DBNull.Value).ToArray());

        return table;
    }

    private static object?[] Row(int idUser, string username, string firstName) => new object?[]
    {
        idUser,
        username,
        firstName
    };
}
