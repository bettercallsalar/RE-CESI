using System.Data;
using System.Data.Common;
using RESR.Core.Controllers.Events;
using RESR.Core.Controllers.Events.Factories;
using RESR.Infrastructure.Events;
using RESR.Infrastructure.Tests.DbFakes;
using RESR.Models.Resources;

namespace RESR.Infrastructure.Tests.Events;

public sealed class MySqlEventRepositoryTests
{
    [Fact]
    public async Task GetByResourceIdAsync_ReturnsEvent_WhenFound()
    {
        var table = CreateEventTable(Row(
            idResource: 11,
            idEvent: 8,
            title: "Conference",
            description: "Desc",
            visibility: "public",
            createdAt: new DateTime(2026, 2, 1),
            modifiedAt: null,
            deletedAt: null,
            idUser: 4,
            idCategory: 2,
            subtitle: "Sub",
            startDate: new DateTime(2026, 3, 10),
            endDate: new DateTime(2026, 3, 11),
            address: "Paris",
            idDepartment: 75
        ));

        var repo = CreateRepo(ReaderCommand(table));

        var @event = await repo.GetByResourceIdAsync(11, CancellationToken.None);

        Assert.NotNull(@event);
        Assert.Equal(11, @event!.IdResource);
        Assert.Equal("Conference", @event.Title);
        Assert.Equal(ResourceVisibility.PUBLIC, @event.Visibility);
    }

    [Fact]
    public async Task CreateAsync_InsertsRowsAndReturnsResourceId()
    {
        var resourceCmd = ScalarCommand(52);
        var eventCmd = NonQueryCommand(1);
        var repo = CreateRepo(resourceCmd, eventCmd);

        var id = await repo.CreateAsync(
            new CreateEventCommand(
                "Conference",
                null,
                ResourceVisibility.PRIVATE,
                2,
                5,
                "Sub",
                new DateTime(2026, 4, 1),
                null,
                "Lyon",
                69),
            CancellationToken.None);

        Assert.Equal(52, id);
        Assert.Contains("@title", resourceCmd.Parameters.Cast<DbParameter>().Select(p => p.ParameterName));
        Assert.Contains("@id_ressource", eventCmd.Parameters.Cast<DbParameter>().Select(p => p.ParameterName));
    }

    [Fact]
    public async Task SoftDeleteAsync_ReturnsTrue_WhenUpdated()
    {
        var repo = CreateRepo(NonQueryCommand(1));

        var deleted = await repo.SoftDeleteAsync(11, CancellationToken.None);

        Assert.True(deleted);
    }

    private static MySqlEventRepository CreateRepo(params FakeDbCommand[] commands)
    {
        DbConnection ConnectionFactory() => new FakeDbConnection(commands);
        return new MySqlEventRepository(ConnectionFactory, new EventFactory());
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

    private static DataTable CreateEventTable(params object?[][] rows)
    {
        var table = new DataTable();
        table.Columns.Add("id_ressource", typeof(int));
        table.Columns.Add("id_event", typeof(int));
        table.Columns.Add("title", typeof(string));
        table.Columns.Add("description", typeof(string));
        table.Columns.Add("visibility", typeof(string));
        table.Columns.Add("created_at", typeof(DateTime));
        table.Columns.Add("modified_at", typeof(DateTime));
        table.Columns.Add("deleted_at", typeof(DateTime));
        table.Columns.Add("id_user", typeof(int));
        table.Columns.Add("id_category", typeof(int));
        table.Columns.Add("subtitle", typeof(string));
        table.Columns.Add("start_date", typeof(DateTime));
        table.Columns.Add("end_date", typeof(DateTime));
        table.Columns.Add("adress", typeof(string));
        table.Columns.Add("id_department", typeof(int));

        foreach (var row in rows)
            table.Rows.Add(row.Select(value => value ?? DBNull.Value).ToArray());

        return table;
    }

    private static object?[] Row(
        int idResource,
        int idEvent,
        string title,
        string? description,
        string visibility,
        DateTime createdAt,
        DateTime? modifiedAt,
        DateTime? deletedAt,
        int idUser,
        int idCategory,
        string? subtitle,
        DateTime startDate,
        DateTime? endDate,
        string? address,
        int? idDepartment) => new object?[]
    {
        idResource,
        idEvent,
        title,
        description,
        visibility,
        createdAt,
        modifiedAt,
        deletedAt,
        idUser,
        idCategory,
        subtitle,
        startDate,
        endDate,
        address,
        idDepartment
    };
}
