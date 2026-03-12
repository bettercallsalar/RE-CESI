using System.Data.Common;
using MySql.Data.MySqlClient;
using RESR.Core.Controllers.Comments.Factories;
using RESR.Core.Controllers.Comments.Ports;
using RESR.Models.Comments;

namespace RESR.Infrastructure.Comments;

public sealed class MySqlCommentRepository : ICommentRepository
{
    private readonly Func<DbConnection> _connectionFactory;
    private readonly ICommentFactory _commentFactory;

    public MySqlCommentRepository(string connectionString, ICommentFactory commentFactory)
    {
        _connectionFactory = () => new MySqlConnection(connectionString);
        _commentFactory = commentFactory;
    }

    internal MySqlCommentRepository(Func<DbConnection> connectionFactory, ICommentFactory commentFactory)
    {
        _connectionFactory = connectionFactory;
        _commentFactory = commentFactory;
    }

    public async Task<bool> ResourceExistsAsync(int idResource, CancellationToken ct)
    {
        const string sql = """
        SELECT COUNT(*)
        FROM `resource`
        WHERE id_ressource = @idResource AND deleted_at IS NULL
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idResource", idResource);

        var result = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result) > 0;
    }

    public async Task<IReadOnlyList<Comment>> GetByResourceIdAsync(int idResource, CancellationToken ct)
    {
        const string sql = """
        SELECT c.id_comment, c.content, c.created_at, c.modified_at, c.deleted_at, c.id_ressource, c.id_user, r.id_comment AS id_parent_comment
        FROM `comment` c
        LEFT JOIN `reply` r ON r.id_comment_post = c.id_comment
        WHERE c.id_ressource = @idResource
        ORDER BY c.created_at ASC, c.id_comment ASC
        """;

        var comments = new List<Comment>();

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idResource", idResource);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            comments.Add(Map(reader));

        return comments;
    }

    public async Task<Comment?> GetByIdAsync(int idComment, CancellationToken ct)
    {
        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        return await GetByIdAsync(conn, idComment, ct);
    }

    public async Task<Comment> CreateAsync(Comment comment, CancellationToken ct)
    {
        const string insertCommentSql = """
        INSERT INTO `comment` (`content`, `id_ressource`, `id_user`)
        VALUES (@content, @idResource, @idUser);
        SELECT LAST_INSERT_ID();
        """;

        const string insertReplySql = """
        INSERT INTO `reply` (`id_comment`, `id_comment_post`)
        VALUES (@idParentComment, @idComment)
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var insertCommentCmd = conn.CreateCommand();
        insertCommentCmd.CommandText = insertCommentSql;
        AddParameter(insertCommentCmd, "@content", comment.Content);
        AddParameter(insertCommentCmd, "@idResource", comment.IdResource);
        AddParameter(insertCommentCmd, "@idUser", comment.IdUser);

        var id = Convert.ToInt32(await insertCommentCmd.ExecuteScalarAsync(ct));

        if (comment.IdParentComment.HasValue)
        {
            await using var insertReplyCmd = conn.CreateCommand();
            insertReplyCmd.CommandText = insertReplySql;
            AddParameter(insertReplyCmd, "@idParentComment", comment.IdParentComment.Value);
            AddParameter(insertReplyCmd, "@idComment", id);
            await insertReplyCmd.ExecuteNonQueryAsync(ct);
        }

        return await GetByIdAsync(conn, id, ct)
            ?? throw new InvalidOperationException("Comment not found after creation");
    }

    public async Task<Comment> UpdateContentAsync(int idComment, string content, CancellationToken ct)
    {
        const string updateSql = """
        UPDATE `comment`
        SET content = @content, modified_at = NOW()
        WHERE id_comment = @idComment AND deleted_at IS NULL
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var updateCmd = conn.CreateCommand();
        updateCmd.CommandText = updateSql;
        AddParameter(updateCmd, "@idComment", idComment);
        AddParameter(updateCmd, "@content", content);

        var rows = await updateCmd.ExecuteNonQueryAsync(ct);
        if (rows == 0)
            throw new InvalidOperationException("Comment not found");

        return await GetByIdAsync(conn, idComment, ct)
            ?? throw new InvalidOperationException("Comment not found after update");
    }

    public async Task<bool> SoftDeleteAsync(int idComment, CancellationToken ct)
    {
        const string sql = """
        UPDATE `comment`
        SET deleted_at = NOW()
        WHERE id_comment = @idComment AND deleted_at IS NULL
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idComment", idComment);

        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }

    private Comment Map(DbDataReader reader) =>
        _commentFactory.CreateFromPersistence(
            Convert.ToInt32(reader["id_comment"]),
            Convert.ToString(reader["content"]) ?? string.Empty,
            Convert.ToDateTime(reader["created_at"]),
            reader["modified_at"] == DBNull.Value ? null : Convert.ToDateTime(reader["modified_at"]),
            reader["deleted_at"] == DBNull.Value ? null : Convert.ToDateTime(reader["deleted_at"]),
            Convert.ToInt32(reader["id_ressource"]),
            Convert.ToInt32(reader["id_user"]),
            reader["id_parent_comment"] == DBNull.Value ? null : Convert.ToInt32(reader["id_parent_comment"])
        );

    private async Task<Comment?> GetByIdAsync(DbConnection conn, int idComment, CancellationToken ct)
    {
        const string sql = """
        SELECT c.id_comment, c.content, c.created_at, c.modified_at, c.deleted_at, c.id_ressource, c.id_user, r.id_comment AS id_parent_comment
        FROM `comment` c
        LEFT JOIN `reply` r ON r.id_comment_post = c.id_comment
        WHERE c.id_comment = @idComment
        """;

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idComment", idComment);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    private static void AddParameter(DbCommand cmd, string name, object? value)
    {
        var parameter = cmd.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        cmd.Parameters.Add(parameter);
    }
}
