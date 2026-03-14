using System.Data.Common;
using System.Text;
using MySql.Data.MySqlClient;
using RESR.Core.Controllers.Events;
using RESR.Core.Controllers.Events.Factories;
using RESR.Core.Controllers.Events.Ports;
using RESR.Models.Departments;
using RESR.Models.Resources;

namespace RESR.Infrastructure.Events;

public sealed class MySqlEventRepository : IEventRepository
{
    private readonly Func<DbConnection> _connectionFactory;
    private readonly IEventFactory _eventFactory;

    public MySqlEventRepository(string connectionString, IEventFactory eventFactory)
    {
        _connectionFactory = () => new MySqlConnection(connectionString);
        _eventFactory = eventFactory;
    }

    internal MySqlEventRepository(Func<DbConnection> connectionFactory, IEventFactory eventFactory)
    {
        _connectionFactory = connectionFactory;
        _eventFactory = eventFactory;
    }

    public async Task<IReadOnlyList<Event>> GetPaginatedAsync(int page, int pageSize, EventListingFilters filters, CancellationToken ct)
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
            e.id_event,
            e.subtitle,
            e.start_date,
            e.end_date,
            e.adress,
            e.id_department,
            e.default_image_id,
            d.name AS department_name,
            d.code AS department_code
        FROM event e
        INNER JOIN resource r ON r.id_ressource = e.id_ressource
        LEFT JOIN department d ON d.id_department = e.id_department
        WHERE 1 = 1
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

        var events = new List<Event>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            events.Add(Map(reader));

        return events;
    }

    public async Task<int> CountAsync(EventListingFilters filters, CancellationToken ct)
    {
        var sql = new StringBuilder("""
        SELECT COUNT(*)
        FROM event e
        INNER JOIN resource r ON r.id_ressource = e.id_ressource
        WHERE 1 = 1
        """);

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        AppendListingFilters(sql, cmd, filters);
        cmd.CommandText = sql.ToString();

        var result = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result);
    }

    public async Task<Event?> GetByResourceIdAsync(int idResource, CancellationToken ct)
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
            e.id_event,
            e.subtitle,
            e.start_date,
            e.end_date,
            e.adress,
            e.id_department,
            e.default_image_id,
            d.name AS department_name,
            d.code AS department_code
        FROM event e
        INNER JOIN resource r ON r.id_ressource = e.id_ressource
        LEFT JOIN department d ON d.id_department = e.id_department
        WHERE r.id_ressource = @id_ressource
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@id_ressource", idResource);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task<int> CreateAsync(CreateEventCommand cmd, CancellationToken ct)
    {
        const string insertResourceSql = """
        INSERT INTO resource (title, description, type, is_approved, visibility, created_at, modified_at, deleted_at, id_user, id_category)
        VALUES (@title, @description, 'event', 0, @visibility, NOW(), NULL, NULL, @id_user, @id_category);
        SELECT LAST_INSERT_ID();
        """;

        const string insertEventSql = """
        INSERT INTO event (subtitle, start_date, end_date, adress, id_department, id_ressource, default_image_id)
        VALUES (@subtitle, @start_date, @end_date, @adress, @id_department, @id_ressource, NULL)
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

        await using var eventCmd = conn.CreateCommand();
        eventCmd.CommandText = insertEventSql;
        AddParameter(eventCmd, "@subtitle", (object?)cmd.Subtitle ?? DBNull.Value);
        AddParameter(eventCmd, "@start_date", cmd.StartDate);
        AddParameter(eventCmd, "@end_date", (object?)cmd.EndDate ?? DBNull.Value);
        AddParameter(eventCmd, "@adress", (object?)cmd.Address ?? DBNull.Value);
        AddParameter(eventCmd, "@id_department", (object?)cmd.IdDepartment ?? DBNull.Value);
        AddParameter(eventCmd, "@id_ressource", idResource);
        await eventCmd.ExecuteNonQueryAsync(ct);

        return idResource;
    }

    public async Task<Event?> PatchAsync(UpdateEventCommand cmd, CancellationToken ct)
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
          AND type = 'event'
          AND deleted_at IS NULL
        """;

        const string updateEventSql = """
        UPDATE event
        SET
            subtitle = COALESCE(@subtitle, subtitle),
            start_date = COALESCE(@start_date, start_date),
            end_date = COALESCE(@end_date, end_date),
            adress = COALESCE(@adress, adress),
            id_department = COALESCE(@id_department, id_department)
        WHERE id_ressource = @id_ressource
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var resourceCmd = conn.CreateCommand();
        resourceCmd.CommandText = updateResourceSql;
        AddPatchResourceParameters(resourceCmd, cmd);
        var affectedResources = await resourceCmd.ExecuteNonQueryAsync(ct);
        if (affectedResources == 0)
            return null;

        await using var eventCmd = conn.CreateCommand();
        eventCmd.CommandText = updateEventSql;
        AddPatchEventParameters(eventCmd, cmd);
        await eventCmd.ExecuteNonQueryAsync(ct);

        return await GetByResourceIdAsync(cmd.IdResource, ct);
    }

    public async Task SetDefaultImageAsync(int idResource, int? defaultImageId, CancellationToken ct)
    {
        const string sql = """
        UPDATE event
        SET default_image_id = @default_image_id
        WHERE id_ressource = @id_ressource
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@id_ressource", idResource);
        AddParameter(cmd, "@default_image_id", (object?)defaultImageId ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<Event?> SetApprovalAsync(SetEventApprovalCommand cmd, CancellationToken ct)
    {
        const string updateResourceSql = """
        UPDATE resource
        SET
            is_approved = @is_approved,
            modified_at = NOW()
        WHERE id_ressource = @id_ressource
          AND type = 'event'
          AND deleted_at IS NULL
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var updateCmd = conn.CreateCommand();
        updateCmd.CommandText = updateResourceSql;
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
          AND type = 'event'
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

    private Event Map(DbDataReader reader)
    {
        return _eventFactory.CreateFromPersistence(
            idResource: Convert.ToInt32(reader["id_ressource"]),
            idEvent: Convert.ToInt32(reader["id_event"]),
            title: Convert.ToString(reader["title"]) ?? string.Empty,
            description: reader["description"] == DBNull.Value ? null : Convert.ToString(reader["description"]),
            visibility: ParseVisibility(Convert.ToString(reader["visibility"])),
            createdAt: Convert.ToDateTime(reader["created_at"]),
            modifiedAt: reader["modified_at"] == DBNull.Value ? null : Convert.ToDateTime(reader["modified_at"]),
            deletedAt: reader["deleted_at"] == DBNull.Value ? null : Convert.ToDateTime(reader["deleted_at"]),
            idUser: Convert.ToInt32(reader["id_user"]),
            idCategory: Convert.ToInt32(reader["id_category"]),
            subtitle: reader["subtitle"] == DBNull.Value ? null : Convert.ToString(reader["subtitle"]),
            startDate: Convert.ToDateTime(reader["start_date"]),
            endDate: reader["end_date"] == DBNull.Value ? null : Convert.ToDateTime(reader["end_date"]),
            address: reader["adress"] == DBNull.Value ? null : Convert.ToString(reader["adress"]),
            department: MapDepartment(reader),
            isApproved: Convert.ToBoolean(reader["is_approved"]),
            defaultImageId: reader["default_image_id"] == DBNull.Value ? null : Convert.ToInt32(reader["default_image_id"])
        );
    }

    private static Department? MapDepartment(DbDataReader reader)
    {
        if (reader["id_department"] == DBNull.Value)
            return null;

        if (reader["department_name"] == DBNull.Value || reader["department_code"] == DBNull.Value)
            throw new InvalidOperationException("Event department data is incomplete. Run latest DB migrations.");

        return new Department
        {
            IdDepartment = Convert.ToInt32(reader["id_department"]),
            Name = Convert.ToString(reader["department_name"]) ?? string.Empty,
            Code = Convert.ToInt32(reader["department_code"])
        };
    }

    private static void AddPatchResourceParameters(DbCommand cmd, UpdateEventCommand @event)
    {
        AddParameter(cmd, "@id_ressource", @event.IdResource);
        AddParameter(cmd, "@title", (object?)@event.Title ?? DBNull.Value);
        AddParameter(cmd, "@description", (object?)@event.Description ?? DBNull.Value);
        AddParameter(cmd, "@visibility", @event.Visibility is null ? DBNull.Value : ToDbVisibility(@event.Visibility.Value));
        AddParameter(cmd, "@id_category", (object?)@event.IdCategory ?? DBNull.Value);
    }

    private static void AddPatchEventParameters(DbCommand cmd, UpdateEventCommand @event)
    {
        AddParameter(cmd, "@id_ressource", @event.IdResource);
        AddParameter(cmd, "@subtitle", (object?)@event.Subtitle ?? DBNull.Value);
        AddParameter(cmd, "@start_date", (object?)@event.StartDate ?? DBNull.Value);
        AddParameter(cmd, "@end_date", (object?)@event.EndDate ?? DBNull.Value);
        AddParameter(cmd, "@adress", (object?)@event.Address ?? DBNull.Value);
        AddParameter(cmd, "@id_department", (object?)@event.IdDepartment ?? DBNull.Value);
    }

    private static void AppendListingFilters(StringBuilder sql, DbCommand cmd, EventListingFilters filters)
    {
        if (!filters.IncludeDeleted)
        {
            sql.AppendLine("  AND r.deleted_at IS NULL");
        }

        if (filters.Keyword is not null)
        {
            sql.AppendLine("""
              AND (
                r.title LIKE @keyword
                OR r.description LIKE @keyword
                OR e.subtitle LIKE @keyword
                OR e.adress LIKE @keyword
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

        if (filters.IdDepartment is not null)
        {
            sql.AppendLine("  AND e.id_department = @id_department");
            AddParameter(cmd, "@id_department", filters.IdDepartment.Value);
        }

        if (filters.IsApproved is not null)
        {
            sql.AppendLine("  AND r.is_approved = @is_approved");
            AddParameter(cmd, "@is_approved", filters.IsApproved.Value);
        }

        if (filters.StartFrom is not null)
        {
            sql.AppendLine("  AND e.start_date >= @start_from");
            AddParameter(cmd, "@start_from", filters.StartFrom.Value);
        }

        if (filters.StartTo is not null)
        {
            sql.AppendLine("  AND e.start_date <= @start_to");
            AddParameter(cmd, "@start_to", filters.StartTo.Value);
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
