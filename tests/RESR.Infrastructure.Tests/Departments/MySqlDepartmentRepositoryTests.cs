using System.Data;
using RESR.Core.Controllers.Departments.Factories;
using RESR.Infrastructure.Departments;
using RESR.Infrastructure.Tests.DbFakes;

namespace RESR.Infrastructure.Tests.Departments;

public sealed class MySqlDepartmentRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsDepartments()
    {
        var table = CreateDepartmentTable(
            Row(1, "IT", 10),
            Row(2, "HR", 20)
        );
        var cmd = ReaderCommand(table);
        var repo = CreateRepo(cmd);

        var list = await repo.GetAllAsync(CancellationToken.None);

        Assert.Equal(2, list.Count);
        Assert.Equal("IT", list[0].Name);
        Assert.Equal("HR", list[1].Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDepartment_WhenFound()
    {
        var table = CreateDepartmentTable(Row(5, "Finance", 30));
        var cmd = ReaderCommand(table);
        var repo = CreateRepo(cmd);

        var department = await repo.GetByIdAsync(5, CancellationToken.None);

        Assert.NotNull(department);
        Assert.Equal(5, department!.IdDepartment);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenMissing()
    {
        var cmd = ReaderCommand(CreateDepartmentTable());
        var repo = CreateRepo(cmd);

        var department = await repo.GetByIdAsync(99, CancellationToken.None);

        Assert.Null(department);
    }

    private static MySqlDepartmentRepository CreateRepo(params FakeDbCommand[] commands)
    {
        return new MySqlDepartmentRepository(() => new FakeDbConnection(commands), new DepartmentFactory());
    }

    private static FakeDbCommand ReaderCommand(DataTable table)
    {
        return new FakeDbCommand
        {
            ExecuteReaderHandler = _ => table.CreateDataReader()
        };
    }

    private static DataTable CreateDepartmentTable(params object?[][] rows)
    {
        var table = new DataTable();
        table.Columns.Add("id_department", typeof(int));
        table.Columns.Add("name", typeof(string));
        table.Columns.Add("code", typeof(int));

        foreach (var row in rows)
            table.Rows.Add(row.Select(value => value ?? DBNull.Value).ToArray());

        return table;
    }

    private static object?[] Row(int id, string name, int code) => new object?[]
    {
        id,
        name,
        code
    };
}
