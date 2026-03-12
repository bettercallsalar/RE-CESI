using System.Data;
using System.Data.Common;
using RESR.Core.Controllers.Reactions.Factories;
using RESR.Infrastructure.Reactions;
using RESR.Infrastructure.Tests.DbFakes;
using RESR.Models.Reactions;

namespace RESR.Infrastructure.Tests.Reactions;

public sealed class MySqlReactionRepositoryTests
{
    [Fact]
    public async Task ResourceExistsAsync_ReturnsTrue_WhenResourceExists()
    {
        var cmd = ScalarCommand(1);
        var repo = CreateRepo(cmd);

        var result = await repo.ResourceExistsAsync(4, CancellationToken.None);

        Assert.True(result);
    }

    [Fact]
    public async Task UserExistsAsync_ReturnsTrue_WhenUserExists()
    {
        var cmd = ScalarCommand(1);
        var repo = CreateRepo(cmd);

        var result = await repo.UserExistsAsync(2, CancellationToken.None);

        Assert.True(result);
    }

    [Fact]
    public async Task GetByResourceIdAsync_ReturnsReactions()
    {
        var table = CreateReactionTable(
            Row(1, ReactionNames.Like, 4, 2, "user2", "User Two"),
            Row(2, ReactionNames.Love, 4, 3, "user3", "User Three")
        );
        var cmd = ReaderCommand(table);
        var repo = CreateRepo(cmd);

        var reactions = await repo.GetByResourceIdAsync(4, CancellationToken.None);

        Assert.Equal(2, reactions.Count);
        Assert.Equal(ReactionNames.Love, reactions[1].Name);
        Assert.Equal("user3", reactions[1].Username);
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsReactions()
    {
        var table = CreateReactionTable(
            Row(1, ReactionNames.Like, 4, 2, "user2", "User Two"),
            Row(2, ReactionNames.Love, 8, 2, "user2", "User Two")
        );
        var cmd = ReaderCommand(table);
        var repo = CreateRepo(cmd);

        var reactions = await repo.GetByUserIdAsync(2, CancellationToken.None);

        Assert.Equal(2, reactions.Count);
        Assert.Equal(8, reactions[1].IdResource);
        Assert.Equal("User Two", reactions[1].FirstName);
    }

    [Fact]
    public async Task GetByResourceAndUserAsync_ReturnsReaction_WhenFound()
    {
        var table = CreateReactionTable(Row(3, ReactionNames.Dislike, 4, 2, "user2", "User Two"));
        var cmd = ReaderCommand(table);
        var repo = CreateRepo(cmd);

        var reaction = await repo.GetByResourceAndUserAsync(4, 2, CancellationToken.None);

        Assert.NotNull(reaction);
        Assert.Equal(3, reaction!.IdReaction);
        Assert.Equal("user2", reaction.Username);
    }

    [Fact]
    public async Task CreateAsync_InsertsAndReturnsReaction()
    {
        var insertCmd = ScalarCommand(12);
        var selectCmd = ReaderCommand(CreateReactionTable(Row(12, ReactionNames.Like, 4, 2, "user2", "User Two")));
        var repo = CreateRepo(insertCmd, selectCmd);

        var result = await repo.CreateAsync(new Reaction
        {
            Name = ReactionNames.Like,
            IdResource = 4,
            IdUser = 2
        }, CancellationToken.None);

        Assert.Equal(12, result.IdReaction);
    }

    [Fact]
    public async Task UpdateNameAsync_UpdatesAndReturnsReaction()
    {
        var updateCmd = NonQueryCommand(1);
        var selectCmd = ReaderCommand(CreateReactionTable(Row(5, ReactionNames.Love, 4, 2, "user2", "User Two")));
        var repo = CreateRepo(updateCmd, selectCmd);

        var result = await repo.UpdateNameAsync(5, ReactionNames.Love, CancellationToken.None);

        Assert.Equal(ReactionNames.Love, result.Name);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNoRowsDeleted()
    {
        var cmd = NonQueryCommand(0);
        var repo = CreateRepo(cmd);

        var result = await repo.DeleteAsync(5, CancellationToken.None);

        Assert.False(result);
    }

    private static MySqlReactionRepository CreateRepo(params FakeDbCommand[] commands)
    {
        DbConnection ConnectionFactory() => new FakeDbConnection(commands);
        return new MySqlReactionRepository(ConnectionFactory, new ReactionFactory());
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

    private static DataTable CreateReactionTable(params object?[][] rows)
    {
        var table = new DataTable();
        table.Columns.Add("id_reaction", typeof(int));
        table.Columns.Add("name", typeof(string));
        table.Columns.Add("id_ressource", typeof(int));
        table.Columns.Add("id_user", typeof(int));
        table.Columns.Add("username", typeof(string));
        table.Columns.Add("first_name", typeof(string));

        foreach (var row in rows)
            table.Rows.Add(row.Select(value => value ?? DBNull.Value).ToArray());

        return table;
    }

    private static object?[] Row(int idReaction, string name, int idResource, int idUser, string username, string firstName)
    {
        return new object?[] { idReaction, name, idResource, idUser, username, firstName };
    }
}
