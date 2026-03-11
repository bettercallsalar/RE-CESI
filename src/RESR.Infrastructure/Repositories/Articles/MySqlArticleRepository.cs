using System.Data.Common;
using MySql.Data.MySqlClient;
using RESR.Core.Controllers.Articles;
using RESR.Core.Controllers.Articles.Factories;
using RESR.Core.Controllers.Articles.Ports;
using RESR.Models.Resources;

namespace RESR.Infrastructure.Articles;

public sealed class MySqlArticleRepository : IArticleRepository
{
    private readonly Func<DbConnection> _connectionFactory;
    private readonly IArticleFactory _articleFactory;

    public MySqlArticleRepository(string connectionString, IArticleFactory articleFactory)
    {
        _connectionFactory = () => new MySqlConnection(connectionString);
        _articleFactory = articleFactory;
    }

    internal MySqlArticleRepository(Func<DbConnection> connectionFactory, IArticleFactory articleFactory)
    {
        _connectionFactory = connectionFactory;
        _articleFactory = articleFactory;
    }

    public async Task<IReadOnlyList<Article>> GetAllAsync(CancellationToken ct)
    {
        const string sql = """
        SELECT
            r.id_ressource,
            r.title,
            r.description,
            r.visibility,
            r.created_at,
            r.modified_at,
            r.deleted_at,
            r.id_user,
            r.id_category,
            a.id_article,
            a.content,
            a.is_approved
        FROM article a
        INNER JOIN resource r ON r.id_ressource = a.id_ressource
        WHERE r.deleted_at IS NULL
        ORDER BY r.id_ressource DESC
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        var articles = new List<Article>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            articles.Add(Map(reader));

        return articles;
    }

    public async Task<Article?> GetByResourceIdAsync(int idResource, CancellationToken ct)
    {
        const string sql = """
        SELECT
            r.id_ressource,
            r.title,
            r.description,
            r.visibility,
            r.created_at,
            r.modified_at,
            r.deleted_at,
            r.id_user,
            r.id_category,
            a.id_article,
            a.content,
            a.is_approved
        FROM article a
        INNER JOIN resource r ON r.id_ressource = a.id_ressource
        WHERE r.id_ressource = @id_ressource
          AND r.deleted_at IS NULL
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@id_ressource", idResource);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task<int> CreateAsync(CreateArticleCommand cmd, CancellationToken ct)
    {
        const string insertResourceSql = """
        INSERT INTO resource (title, description, type, visibility, created_at, modified_at, deleted_at, id_user, id_category)
        VALUES (@title, @description, 'article', @visibility, NOW(), NULL, NULL, @id_user, @id_category);
        SELECT LAST_INSERT_ID();
        """;

        const string insertArticleSql = """
        INSERT INTO article (content, is_approved, id_ressource)
        VALUES (@content, 0, @id_ressource)
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var resourceCmd = conn.CreateCommand();
        resourceCmd.CommandText = insertResourceSql;
        AddParameter(resourceCmd, "@title", cmd.Title);
        AddParameter(resourceCmd, "@description", (object?)cmd.Description ?? DBNull.Value);
        AddParameter(resourceCmd, "@visibility", ToDbVisibility(cmd.Visibility));
        AddParameter(resourceCmd, "@id_user", cmd.IdUser);
        AddParameter(resourceCmd, "@id_category", cmd.IdCategory);

        var idResourceObj = await resourceCmd.ExecuteScalarAsync(ct);
        var idResource = Convert.ToInt32(idResourceObj);

        await using var articleCmd = conn.CreateCommand();
        articleCmd.CommandText = insertArticleSql;
        AddParameter(articleCmd, "@content", cmd.Content);
        AddParameter(articleCmd, "@id_ressource", idResource);
        await articleCmd.ExecuteNonQueryAsync(ct);

        return idResource;
    }

    public async Task<Article?> PatchAsync(UpdateArticleCommand cmd, CancellationToken ct)
    {
        const string updateResourceSql = """
        UPDATE resource
        SET
            title = COALESCE(@title, title),
            description = COALESCE(@description, description),
            visibility = COALESCE(@visibility, visibility),
            id_category = COALESCE(@id_category, id_category),
            modified_at = NOW()
        WHERE id_ressource = @id_ressource
          AND type = 'article'
          AND deleted_at IS NULL
        """;

        const string updateArticleSql = """
        UPDATE article
        SET
            content = COALESCE(@content, content)
        WHERE id_ressource = @id_ressource
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var resourceCmd = conn.CreateCommand();
        resourceCmd.CommandText = updateResourceSql;
        AddPatchParameters(resourceCmd, cmd);
        var affectedResources = await resourceCmd.ExecuteNonQueryAsync(ct);
        if (affectedResources == 0)
            return null;

        await using var articleCmd = conn.CreateCommand();
        articleCmd.CommandText = updateArticleSql;
        AddParameter(articleCmd, "@id_ressource", cmd.IdResource);
        AddParameter(articleCmd, "@content", (object?)cmd.Content ?? DBNull.Value);
        await articleCmd.ExecuteNonQueryAsync(ct);

        return await GetByResourceIdAsync(cmd.IdResource, ct);
    }

    public async Task<Article?> SetApprovalAsync(SetArticleApprovalCommand cmd, CancellationToken ct)
    {
        const string updateArticleSql = """
        UPDATE article a
        INNER JOIN resource r ON r.id_ressource = a.id_ressource
        SET a.is_approved = @is_approved
        WHERE a.id_ressource = @id_ressource
          AND r.type = 'article'
          AND r.deleted_at IS NULL
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var updateCmd = conn.CreateCommand();
        updateCmd.CommandText = updateArticleSql;
        AddParameter(updateCmd, "@id_ressource", cmd.IdResource);
        AddParameter(updateCmd, "@is_approved", cmd.IsApproved);
        var affectedRows = await updateCmd.ExecuteNonQueryAsync(ct);
        if (affectedRows == 0)
            return null;

        return await GetByResourceIdAsync(cmd.IdResource, ct);
    }

    public async Task<bool> SoftDeleteAsync(int idResource, CancellationToken ct)
    {
        const string sql = """
        UPDATE resource
        SET deleted_at = NOW()
        WHERE id_ressource = @id_ressource
          AND type = 'article'
          AND deleted_at IS NULL
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@id_ressource", idResource);

        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }

    private Article Map(DbDataReader reader)
    {
        return _articleFactory.CreateFromPersistence(
            idResource: Convert.ToInt32(reader["id_ressource"]),
            idArticle: Convert.ToInt32(reader["id_article"]),
            title: Convert.ToString(reader["title"]) ?? string.Empty,
            description: reader["description"] == DBNull.Value ? null : Convert.ToString(reader["description"]),
            visibility: ParseVisibility(Convert.ToString(reader["visibility"])),
            createdAt: Convert.ToDateTime(reader["created_at"]),
            modifiedAt: reader["modified_at"] == DBNull.Value ? null : Convert.ToDateTime(reader["modified_at"]),
            deletedAt: reader["deleted_at"] == DBNull.Value ? null : Convert.ToDateTime(reader["deleted_at"]),
            idUser: Convert.ToInt32(reader["id_user"]),
            idCategory: Convert.ToInt32(reader["id_category"]),
            content: Convert.ToString(reader["content"]) ?? string.Empty,
            isApproved: Convert.ToBoolean(reader["is_approved"])
        );
    }

    private static void AddPatchParameters(DbCommand cmd, UpdateArticleCommand article)
    {
        AddParameter(cmd, "@id_ressource", article.IdResource);
        AddParameter(cmd, "@title", (object?)article.Title ?? DBNull.Value);
        AddParameter(cmd, "@description", (object?)article.Description ?? DBNull.Value);
        AddParameter(cmd, "@visibility", article.Visibility is null ? DBNull.Value : ToDbVisibility(article.Visibility.Value));
        AddParameter(cmd, "@id_category", (object?)article.IdCategory ?? DBNull.Value);
    }

    private static ResourceVisibility ParseVisibility(string? visibility)
    {
        return visibility?.Equals("private", StringComparison.OrdinalIgnoreCase) == true
            ? ResourceVisibility.PRIVATE
            : ResourceVisibility.PUBLIC;
    }

    private static string ToDbVisibility(ResourceVisibility visibility)
    {
        return visibility == ResourceVisibility.PRIVATE ? "private" : "public";
    }

    private static void AddParameter(DbCommand cmd, string name, object? value)
    {
        var parameter = cmd.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        cmd.Parameters.Add(parameter);
    }
}
