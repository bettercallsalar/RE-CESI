using System.Data;
using System.Data.Common;
using RESR.Core.Controllers.Articles;
using RESR.Core.Controllers.Articles.Factories;
using RESR.Infrastructure.Articles;
using RESR.Infrastructure.Tests.DbFakes;
using RESR.Models.Resources;

namespace RESR.Infrastructure.Tests.Articles;

public sealed class MySqlArticleRepositoryTests
{
    [Fact]
    public async Task GetPaginatedAsync_AppliesFilters_AndReturnsRows()
    {
        var table = CreateArticleTable(Row(
            idResource: 10,
            idArticle: 7,
            title: "Post",
            description: "Desc",
            visibility: "private",
            createdAt: new DateTime(2026, 1, 1),
            modifiedAt: null,
            deletedAt: null,
            idUser: 2,
            idCategory: 3,
            content: "Body",
            isApproved: true
        ));
        var cmd = ReaderCommand(table);
        var repo = CreateRepo(cmd);

        var list = await repo.GetPaginatedAsync(
            2,
            5,
            new ArticleListingFilters("post", ResourceVisibility.PRIVATE, 2, 3, true, new DateTime(2026, 1, 1), new DateTime(2026, 1, 31)),
            CancellationToken.None);

        Assert.Single(list);
        var names = cmd.Parameters.Cast<DbParameter>().Select(p => p.ParameterName).ToList();
        Assert.Contains("@keyword", names);
        Assert.Contains("@visibility", names);
        Assert.Contains("@id_user", names);
        Assert.Contains("@id_category", names);
        Assert.Contains("@is_approved", names);
        Assert.Contains("@created_from", names);
        Assert.Contains("@created_to", names);
        Assert.Contains("@limit", names);
        Assert.Contains("@offset", names);
    }

    [Fact]
    public async Task CountAsync_ReturnsCount()
    {
        var cmd = ScalarCommand(8);
        var repo = CreateRepo(cmd);

        var count = await repo.CountAsync(
            new ArticleListingFilters("post", ResourceVisibility.PUBLIC, null, null, false, null, null),
            CancellationToken.None);

        Assert.Equal(8, count);
        Assert.Contains("@keyword", cmd.Parameters.Cast<DbParameter>().Select(p => p.ParameterName));
    }

    [Fact]
    public async Task GetByResourceIdAsync_ReturnsArticle_WhenFound()
    {
        var table = CreateArticleTable(Row(
            idResource: 10,
            idArticle: 7,
            title: "Post",
            description: "Desc",
            visibility: "private",
            createdAt: new DateTime(2026, 1, 1),
            modifiedAt: null,
            deletedAt: null,
            idUser: 2,
            idCategory: 3,
            content: "Body",
            isApproved: false
        ));

        var repo = CreateRepo(ReaderCommand(table));

        var article = await repo.GetByResourceIdAsync(10, CancellationToken.None);

        Assert.NotNull(article);
        Assert.Equal(10, article!.IdResource);
        Assert.Equal(7, article.IdArticle);
        Assert.Equal(ResourceVisibility.PRIVATE, article.Visibility);
    }

    [Fact]
    public async Task CreateAsync_InsertsRowsAndReturnsResourceId()
    {
        var resourceCmd = ScalarCommand(44);
        var articleCmd = NonQueryCommand(1);
        var repo = CreateRepo(resourceCmd, articleCmd);

        var id = await repo.CreateAsync(
            new CreateArticleCommand(
                "Post",
                "Desc",
                ResourceVisibility.PUBLIC,
                1,
                2,
                "Body"),
            CancellationToken.None);

        Assert.Equal(44, id);
        Assert.Contains("@title", resourceCmd.Parameters.Cast<DbParameter>().Select(p => p.ParameterName));
        Assert.Contains("@id_ressource", articleCmd.Parameters.Cast<DbParameter>().Select(p => p.ParameterName));
    }

    [Fact]
    public async Task SoftDeleteAsync_ReturnsFalse_WhenNothingUpdated()
    {
        var repo = CreateRepo(NonQueryCommand(0));

        var deleted = await repo.SoftDeleteAsync(55, CancellationToken.None);

        Assert.False(deleted);
    }

    [Fact]
    public async Task SetApprovalAsync_UpdatesApproval()
    {
        var table = CreateArticleTable(Row(
            idResource: 10,
            idArticle: 7,
            title: "Post",
            description: "Desc",
            visibility: "public",
            createdAt: new DateTime(2026, 1, 1),
            modifiedAt: null,
            deletedAt: null,
            idUser: 2,
            idCategory: 3,
            content: "Body",
            isApproved: true
        ));
        var cmd = new FakeDbCommand
        {
            ExecuteNonQueryHandler = _ => 1,
            ExecuteReaderHandler = _ => table.CreateDataReader()
        };
        var repo = CreateRepo(cmd);

        var article = await repo.SetApprovalAsync(new SetArticleApprovalCommand(10, true), CancellationToken.None);

        Assert.NotNull(article);
        Assert.True(article!.IsApproved);
    }

    private static MySqlArticleRepository CreateRepo(params FakeDbCommand[] commands)
    {
        DbConnection ConnectionFactory() => new FakeDbConnection(commands);
        return new MySqlArticleRepository(ConnectionFactory, new ArticleFactory());
    }

    private static FakeDbCommand ReaderCommand(DataTable table) =>
        new()
        {
            ExecuteReaderHandler = _ => table.CreateDataReader()
        };

    private static FakeDbCommand ScalarCommand(object result) =>
        new()
        {
            ExecuteScalarHandler = _ => result
        };

    private static FakeDbCommand NonQueryCommand(int rows) =>
        new()
        {
            ExecuteNonQueryHandler = _ => rows
        };

    private static DataTable CreateArticleTable(params object?[][] rows)
    {
        var table = new DataTable();
        table.Columns.Add("id_ressource", typeof(int));
        table.Columns.Add("id_article", typeof(int));
        table.Columns.Add("title", typeof(string));
        table.Columns.Add("description", typeof(string));
        table.Columns.Add("visibility", typeof(string));
        table.Columns.Add("created_at", typeof(DateTime));
        table.Columns.Add("modified_at", typeof(DateTime));
        table.Columns.Add("deleted_at", typeof(DateTime));
        table.Columns.Add("id_user", typeof(int));
        table.Columns.Add("id_category", typeof(int));
        table.Columns.Add("content", typeof(string));
        table.Columns.Add("is_approved", typeof(bool));

        foreach (var row in rows)
            table.Rows.Add(row.Select(value => value ?? DBNull.Value).ToArray());

        return table;
    }

    private static object?[] Row(
        int idResource,
        int idArticle,
        string title,
        string? description,
        string visibility,
        DateTime createdAt,
        DateTime? modifiedAt,
        DateTime? deletedAt,
        int idUser,
        int idCategory,
        string content,
        bool isApproved) => new object?[]
    {
        idResource,
        idArticle,
        title,
        description,
        visibility,
        createdAt,
        modifiedAt,
        deletedAt,
        idUser,
        idCategory,
        content,
        isApproved
    };
}
