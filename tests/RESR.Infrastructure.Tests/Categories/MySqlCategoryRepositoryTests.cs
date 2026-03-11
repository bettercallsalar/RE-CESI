using System.Data;
using RESR.Core.Controllers.Categories.Factories;
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
