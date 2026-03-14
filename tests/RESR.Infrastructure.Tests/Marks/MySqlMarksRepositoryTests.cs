using System.Data;
using System.Data.Common;
using RESR.Infrastructure.Marks;
using RESR.Infrastructure.Tests.DbFakes;
using RESR.Models.Marks;

namespace RESR.Infrastructure.Tests.Marks;

public sealed class MySqlMarksRepositoryTests
{
    [Fact]
    public async Task ResourceExistsAsync_ReturnsTrue_WhenCountPositive()
    {
        var cmd = ScalarCommand(1);
        var repo = CreateRepo(cmd);

        var exists = await repo.ResourceExistsAsync(4, CancellationToken.None);

        Assert.True(exists);
        Assert.Contains("resource", cmd.CommandText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("@idResource", cmd.Parameters.Cast<DbParameter>().Select(p => p.ParameterName));
    }

    [Fact]
    public async Task ResourceExistsAsync_ReturnsFalse_WhenCountZero()
    {
        var cmd = ScalarCommand(0);
        var repo = CreateRepo(cmd);

        var exists = await repo.ResourceExistsAsync(4, CancellationToken.None);

        Assert.False(exists);
    }

    [Fact]
    public async Task MarkAsFavoriteAsync_InsertsAndReturnsMark()
    {
        var insertCmd = NonQueryCommand(1);
        var selectCmd = ReaderCommand(CreateMarkTable(Row(10, true, false, 4, 2)));
        var repo = CreateRepo(insertCmd, selectCmd);

        var mark = await repo.MarkAsFavoriteAsync(4, 2, CancellationToken.None);

        Assert.Equal(10, mark.IdMark);
        Assert.True(mark.IsFavorite);
        Assert.Equal(4, mark.IdRessource);
        var names = insertCmd.Parameters.Cast<DbParameter>().Select(p => p.ParameterName).ToList();
        Assert.Contains("@idResource", names);
        Assert.Contains("@idUser", names);
    }

    [Fact]
    public async Task MarkAsFavoriteAsync_Throws_WhenNoRowReturned()
    {
        var insertCmd = NonQueryCommand(1);
        var selectCmd = ReaderCommand(CreateMarkTable());
        var repo = CreateRepo(insertCmd, selectCmd);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.MarkAsFavoriteAsync(4, 2, CancellationToken.None));
    }

    [Fact]
    public async Task UnmarkAsFavoriteAsync_ReturnsFalse_WhenNoRowsUpdated()
    {
        var updateCmd = NonQueryCommand(0);
        var repo = CreateRepo(updateCmd);

        var removed = await repo.UnmarkAsFavoriteAsync(4, 2, CancellationToken.None);

        Assert.False(removed);
    }

    [Fact]
    public async Task UnmarkAsFavoriteAsync_CleansUp_WhenUpdated()
    {
        var updateCmd = NonQueryCommand(1);
        var cleanupCmd = NonQueryCommand(1);
        var repo = CreateRepo(updateCmd, cleanupCmd);

        var removed = await repo.UnmarkAsFavoriteAsync(4, 2, CancellationToken.None);

        Assert.True(removed);
        Assert.Contains("UPDATE `mark`", updateCmd.CommandText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("DELETE FROM `mark`", cleanupCmd.CommandText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task MarkAsReadLaterAsync_InsertsAndReturnsMark()
    {
        var insertCmd = NonQueryCommand(1);
        var selectCmd = ReaderCommand(CreateMarkTable(Row(11, false, true, 4, 2)));
        var repo = CreateRepo(insertCmd, selectCmd);

        var mark = await repo.MarkAsReadLaterAsync(4, 2, CancellationToken.None);

        Assert.Equal(11, mark.IdMark);
        Assert.True(mark.IsReadLater);
    }

    [Fact]
    public async Task MarkAsReadLaterAsync_Throws_WhenNoRowReturned()
    {
        var insertCmd = NonQueryCommand(1);
        var selectCmd = ReaderCommand(CreateMarkTable());
        var repo = CreateRepo(insertCmd, selectCmd);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.MarkAsReadLaterAsync(4, 2, CancellationToken.None));
    }

    [Fact]
    public async Task UnmarkAsReadLaterAsync_ReturnsFalse_WhenNoRowsUpdated()
    {
        var updateCmd = NonQueryCommand(0);
        var repo = CreateRepo(updateCmd);

        var removed = await repo.UnmarkAsReadLaterAsync(4, 2, CancellationToken.None);

        Assert.False(removed);
    }

    [Fact]
    public async Task UnmarkAsReadLaterAsync_CleansUp_WhenUpdated()
    {
        var updateCmd = NonQueryCommand(1);
        var cleanupCmd = NonQueryCommand(1);
        var repo = CreateRepo(updateCmd, cleanupCmd);

        var removed = await repo.UnmarkAsReadLaterAsync(4, 2, CancellationToken.None);

        Assert.True(removed);
        Assert.Contains("UPDATE `mark`", updateCmd.CommandText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("DELETE FROM `mark`", cleanupCmd.CommandText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetFavoriteRessourcesAsync_ReturnsList()
    {
        var cmd = ReaderCommand(CreateMarkTable(
            Row(1, true, false, 4, 2),
            Row(2, true, false, 5, 2)
        ));
        var repo = CreateRepo(cmd);

        var list = await repo.GetFavoriteRessourcesAsync(2, CancellationToken.None);

        Assert.Equal(2, list.Count);
        Assert.All(list, m => Assert.True(m.IsFavorite));
    }

    [Fact]
    public async Task GetReadLaterRessourcesAsync_ReturnsList()
    {
        var cmd = ReaderCommand(CreateMarkTable(
            Row(3, false, true, 7, 2),
            Row(4, false, true, 8, 2)
        ));
        var repo = CreateRepo(cmd);

        var list = await repo.GetReadLaterRessourcesAsync(2, CancellationToken.None);

        Assert.Equal(2, list.Count);
        Assert.All(list, m => Assert.True(m.IsReadLater));
    }

    [Fact]
    public async Task GetFavoriteRessourceAsync_ReturnsMark_WhenFound()
    {
        var cmd = ReaderCommand(CreateMarkTable(Row(5, true, false, 9, 2)));
        var repo = CreateRepo(cmd);

        var mark = await repo.GetFavoriteRessourceAsync(9, 2, CancellationToken.None);

        Assert.NotNull(mark);
        Assert.True(mark!.IsFavorite);
    }

    [Fact]
    public async Task GetFavoriteRessourceAsync_ReturnsNull_WhenMissing()
    {
        var cmd = ReaderCommand(CreateMarkTable());
        var repo = CreateRepo(cmd);

        var mark = await repo.GetFavoriteRessourceAsync(9, 2, CancellationToken.None);

        Assert.Null(mark);
    }

    [Fact]
    public async Task GetReadLaterRessourceAsync_ReturnsMark_WhenFound()
    {
        var cmd = ReaderCommand(CreateMarkTable(Row(6, false, true, 9, 2)));
        var repo = CreateRepo(cmd);

        var mark = await repo.GetReadLaterRessourceAsync(9, 2, CancellationToken.None);

        Assert.NotNull(mark);
        Assert.True(mark!.IsReadLater);
    }

    [Fact]
    public async Task GetReadLaterRessourceAsync_ReturnsNull_WhenMissing()
    {
        var cmd = ReaderCommand(CreateMarkTable());
        var repo = CreateRepo(cmd);

        var mark = await repo.GetReadLaterRessourceAsync(9, 2, CancellationToken.None);

        Assert.Null(mark);
    }

    private static MySqlMarksRepository CreateRepo(params FakeDbCommand[] commands)
    {
        DbConnection ConnectionFactory() => new FakeDbConnection(commands);
        return new MySqlMarksRepository(ConnectionFactory);
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

    private static DataTable CreateMarkTable(params object?[][] rows)
    {
        var table = new DataTable();
        table.Columns.Add("id_mark", typeof(int));
        table.Columns.Add("is_favorite", typeof(bool));
        table.Columns.Add("is_read_later", typeof(bool));
        table.Columns.Add("id_ressource", typeof(int));
        table.Columns.Add("id_user", typeof(int));

        foreach (var row in rows)
            table.Rows.Add(row.Select(value => value ?? DBNull.Value).ToArray());

        return table;
    }

    private static object?[] Row(
        int idMark,
        bool isFavorite,
        bool isReadLater,
        int idResource,
        int idUser
    ) => new object?[]
    {
        idMark,
        isFavorite,
        isReadLater,
        idResource,
        idUser
    };
}
