using System.Data.Common;
using System.Text;
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

    public async Task<IReadOnlyList<Article>> GetPaginatedAsync(int page, int pageSize, ArticleListingFilters filters, CancellationToken ct)
    {
        var offset = (page - 1) * pageSize;
        var sql = new StringBuilder("""
        SELECT
            r.id_ressource,
            r.title,
            r.description,
            r.is_approved,
            r.visibility,
            r.created_at,
            r.modified_at,
            r.deleted_at,
            r.id_user,
            r.id_category,
            a.id_article,
            a.content
        FROM article a
        INNER JOIN resource r ON r.id_ressource = a.id_ressource
        WHERE r.deleted_at IS NULL
        """);

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        AppendListingFilters(sql, cmd, filters);
        sql.AppendLine("ORDER BY r.id_ressource DESC");
        sql.AppendLine("LIMIT @limit OFFSET @offset");
        cmd.CommandText = sql.ToString();
        AddParameter(cmd, "@limit", pageSize);
        AddParameter(cmd, "@offset", offset);

        var articles = new List<Article>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            articles.Add(Map(reader));

        return articles;
    }

    public async Task<int> CountAsync(ArticleListingFilters filters, CancellationToken ct)
    {
        var sql = new StringBuilder("""
        SELECT COUNT(*)
        FROM article a
        INNER JOIN resource r ON r.id_ressource = a.id_ressource
        WHERE r.deleted_at IS NULL
        """);

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        AppendListingFilters(sql, cmd, filters);
        cmd.CommandText = sql.ToString();

        var result = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result);
    }

    public async Task<Article?> GetByResourceIdAsync(int idResource, CancellationToken ct)
    {
        const string sql = """
        SELECT
            r.id_ressource,
            r.title,
            r.description,
            r.is_approved,
            r.visibility,
            r.created_at,
            r.modified_at,
            r.deleted_at,
            r.id_user,
            r.id_category,
            a.id_article,
            a.content
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
        INSERT INTO resource (title, description, type, is_approved, visibility, created_at, modified_at, deleted_at, id_user, id_category)
        VALUES (@title, @description, 'article', 0, @visibility, NOW(), NULL, NULL, @id_user, @id_category);
        SELECT LAST_INSERT_ID();
        """;

        const string insertArticleSql = """
        INSERT INTO article (content, id_ressource)
        VALUES (@content, @id_ressource)
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
        UPDATE resource
        SET
            is_approved = @is_approved,
            modified_at = NOW()
        WHERE id_ressource = @id_ressource
          AND type = 'article'
          AND deleted_at IS NULL
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

    private static void AppendListingFilters(StringBuilder sql, DbCommand cmd, ArticleListingFilters filters)
    {
        if (filters.Keyword is not null)
        {
            sql.AppendLine("""
              AND (
                r.title LIKE @keyword
                OR r.description LIKE @keyword
                OR a.content LIKE @keyword
              )
            """);
            AddParameter(cmd, "@keyword", $"%{filters.Keyword}%");
        }

        if (filters.Visibility is not null)
        {
            sql.AppendLine("  AND r.visibility = @visibility");
            AddParameter(cmd, "@visibility", ToDbVisibility(filters.Visibility.Value));
        }

        if (filters.IdUser is not null)
        {
            sql.AppendLine("  AND r.id_user = @id_user");
            AddParameter(cmd, "@id_user", filters.IdUser.Value);
        }

        if (filters.IdCategory is not null)
        {
            sql.AppendLine("  AND r.id_category = @id_category");
            AddParameter(cmd, "@id_category", filters.IdCategory.Value);
        }

        if (filters.IsApproved is not null)
        {
            sql.AppendLine("  AND r.is_approved = @is_approved");
            AddParameter(cmd, "@is_approved", filters.IsApproved.Value);
        }

        if (filters.CreatedFrom is not null)
        {
            sql.AppendLine("  AND r.created_at >= @created_from");
            AddParameter(cmd, "@created_from", filters.CreatedFrom.Value);
        }

        if (filters.CreatedTo is not null)
        {
            sql.AppendLine("  AND r.created_at <= @created_to");
            AddParameter(cmd, "@created_to", filters.CreatedTo.Value);
        }
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
