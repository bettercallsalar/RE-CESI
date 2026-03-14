using System.Data;
using System.Data.Common;
using RESR.Core.Controllers.Comments.Factories;
using RESR.Infrastructure.Comments;
using RESR.Infrastructure.Tests.DbFakes;

namespace RESR.Infrastructure.Tests.Comments;

public sealed class MySqlCommentRepositoryTests
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
    public async Task GetByResourceIdAsync_ReturnsComments()
    {
        var table = CreateCommentTable(
            Row(1, "Hello", new DateTime(2026, 3, 11, 12, 0, 0), null, null, 4, 2, null),
            Row(2, "Reply", new DateTime(2026, 3, 11, 12, 5, 0), null, new DateTime(2026, 3, 11, 13, 0, 0), 4, 3, 1)
        );
        var cmd = ReaderCommand(table);
        var repo = CreateRepo(cmd);

        var comments = await repo.GetByResourceIdAsync(4, CancellationToken.None);

        Assert.Equal(2, comments.Count);
        Assert.Equal(1, comments[1].IdParentComment);
        Assert.NotNull(comments[1].DeletedAt);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsComment_WhenFound()
    {
        var table = CreateCommentTable(Row(3, "Hello", new DateTime(2026, 3, 11, 12, 0, 0), null, null, 4, 2, 1));
        var cmd = ReaderCommand(table);
        var repo = CreateRepo(cmd);

        var comment = await repo.GetByIdAsync(3, CancellationToken.None);

        Assert.NotNull(comment);
        Assert.Equal(3, comment!.IdComment);
        Assert.Equal(1, comment.IdParentComment);
    }

    [Fact]
    public async Task CreateAsync_InsertsComment_AndReply_WhenParentProvided()
    {
        var insertCommentCmd = ScalarCommand(12);
        var insertReplyCmd = NonQueryCommand(1);
        var selectCmd = ReaderCommand(CreateCommentTable(Row(12, "Hello", new DateTime(2026, 3, 11, 12, 0, 0), null, null, 4, 2, 1)));
        var repo = CreateRepo(insertCommentCmd, insertReplyCmd, selectCmd);

        var result = await repo.CreateAsync(new RESR.Models.Comments.Comment
        {
            Content = "Hello",
            IdResource = 4,
            IdUser = 2,
            IdParentComment = 1
        }, CancellationToken.None);

        Assert.Equal(12, result.IdComment);
        Assert.Contains("@idParentComment", insertReplyCmd.Parameters.Cast<DbParameter>().Select(p => p.ParameterName));
    }

    [Fact]
    public async Task UpdateContentAsync_UpdatesAndReturnsComment()
    {
        var updateCmd = NonQueryCommand(1);
        var selectCmd = ReaderCommand(CreateCommentTable(Row(5, "Updated", new DateTime(2026, 3, 11, 12, 0, 0), new DateTime(2026, 3, 11, 12, 1, 0), null, 4, 2, null)));
        var repo = CreateRepo(updateCmd, selectCmd);

        var result = await repo.UpdateContentAsync(5, "Updated", CancellationToken.None);

        Assert.Equal("Updated", result.Content);
    }

    [Fact]
    public async Task SoftDeleteAsync_ReturnsFalse_WhenNoRowsUpdated()
    {
        var cmd = NonQueryCommand(0);
        var repo = CreateRepo(cmd);

        var result = await repo.SoftDeleteAsync(5, CancellationToken.None);

        Assert.False(result);
    }

    private static MySqlCommentRepository CreateRepo(params FakeDbCommand[] commands)
    {
        DbConnection ConnectionFactory() => new FakeDbConnection(commands);
        return new MySqlCommentRepository(ConnectionFactory, new CommentFactory());
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

    private static DataTable CreateCommentTable(params object?[][] rows)
    {
        var table = new DataTable();
        table.Columns.Add("id_comment", typeof(int));
        table.Columns.Add("content", typeof(string));
        table.Columns.Add("created_at", typeof(DateTime));
        table.Columns.Add("modified_at", typeof(DateTime));
        table.Columns.Add("deleted_at", typeof(DateTime));
        table.Columns.Add("id_ressource", typeof(int));
        table.Columns.Add("id_user", typeof(int));
        table.Columns.Add("id_parent_comment", typeof(int));

        foreach (var row in rows)
            table.Rows.Add(row.Select(value => value ?? DBNull.Value).ToArray());

        return table;
    }

    private static object?[] Row(
        int idComment,
        string content,
        DateTime createdAt,
        DateTime? modifiedAt,
        DateTime? deletedAt,
        int idResource,
        int idUser,
        int? idParentComment)
    {
        return new object?[]
        {
            idComment,
            content,
            createdAt,
            modifiedAt,
            deletedAt,
            idResource,
            idUser,
            idParentComment
        };
    }
}
