using System.Data;
using System.Data.Common;
using RESR.Core.Controllers.Categories.Factories;
using RESR.Core.Controllers.Categories;
using RESR.Infrastructure.Categories;
using RESR.Infrastructure.Tests.DbFakes;

namespace RESR.Infrastructure.Tests.Categories;

public sealed class MySqlCategoryRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsCategories()
    {
        var table = CreateCategoryTable(
            Row(1, "Atelier"),
            Row(2, "Conference")
        );
        var cmd = ReaderCommand(table);
        var repo = CreateRepo(cmd);

        var list = await repo.GetAllAsync(CancellationToken.None);

        Assert.Equal(2, list.Count);
        Assert.Equal("Atelier", list[0].Name);
        Assert.Equal("Conference", list[1].Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCategory_WhenFound()
    {
        var table = CreateCategoryTable(Row(5, "Salon"));
        var cmd = ReaderCommand(table);
        var repo = CreateRepo(cmd);

        var category = await repo.GetByIdAsync(5, CancellationToken.None);

        Assert.NotNull(category);
        Assert.Equal(5, category!.IdCategory);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenMissing()
    {
        var cmd = ReaderCommand(CreateCategoryTable());
        var repo = CreateRepo(cmd);

        var category = await repo.GetByIdAsync(99, CancellationToken.None);

        Assert.Null(category);
    }

    [Fact]
    public async Task GetFavoriteCategoriesAsync_ReturnsUserFavorites()
    {
        var table = CreateCategoryTable(Row(3, "Salon"));
        var cmd = ReaderCommand(table);
        var repo = CreateRepo(cmd);

        var categories = await repo.GetFavoriteCategoriesAsync(7, CancellationToken.None);

        Assert.Single(categories);
        Assert.Contains("@idUser", cmd.Parameters.Cast<DbParameter>().Select(p => p.ParameterName));
    }

    [Fact]
    public async Task AddToUserAsync_UsesProvidedUserId()
    {
        var cmd = NonQueryCommand(1);
        var repo = CreateRepo(cmd);

        var result = await repo.AddToUserAsync(7, 2, CancellationToken.None);

        Assert.Equal(AddToUserResult.Added, result);
        Assert.Contains(cmd.Parameters.Cast<DbParameter>(), p => p.ParameterName == "@idUser" && Equals(p.Value, 7));
        Assert.Contains(cmd.Parameters.Cast<DbParameter>(), p => p.ParameterName == "@idCategory" && Equals(p.Value, 2));
    }

    [Fact]
    public async Task RemoveFromUserAsync_UsesProvidedUserId()
    {
        var cmd = NonQueryCommand(1);
        var repo = CreateRepo(cmd);

        var removed = await repo.RemoveFromUserAsync(7, 2, CancellationToken.None);

        Assert.True(removed);
        Assert.Contains(cmd.Parameters.Cast<DbParameter>(), p => p.ParameterName == "@idUser" && Equals(p.Value, 7));
        Assert.Contains(cmd.Parameters.Cast<DbParameter>(), p => p.ParameterName == "@idCategory" && Equals(p.Value, 2));
    }

    private static MySqlCategoryRepository CreateRepo(params FakeDbCommand[] commands)
    {
        return new MySqlCategoryRepository(() => new FakeDbConnection(commands), new CategoryFactory());
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

    private static DataTable CreateCategoryTable(params object?[][] rows)
    {
        var table = new DataTable();
        table.Columns.Add("id_category", typeof(int));
        table.Columns.Add("name", typeof(string));

        foreach (var row in rows)
            table.Rows.Add(row.Select(value => value ?? DBNull.Value).ToArray());

        return table;
    }

    private static object?[] Row(int id, string name) => new object?[]
    {
        id,
        name
    };
}
