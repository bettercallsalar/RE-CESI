using MySql.Data.MySqlClient;
using RESR.Core.Controllers.Departments.Factories;
using RESR.Core.Controllers.Departments.Ports;
using RESR.Models.Departments;
using System.Data.Common;

namespace RESR.Infrastructure.Departments;

public sealed class MySqlDepartmentRepository : IDepartmentRepository
{
    private readonly Func<DbConnection> _connectionFactory;
    private readonly IDepartmentFactory _departmentFactory;

    public MySqlDepartmentRepository(string connectionString, IDepartmentFactory departmentFactory)
    {
        _connectionFactory = () => new MySqlConnection(connectionString);
        _departmentFactory = departmentFactory;
    }

    internal MySqlDepartmentRepository(Func<DbConnection> connectionFactory, IDepartmentFactory departmentFactory)
    {
        _connectionFactory = connectionFactory;
        _departmentFactory = departmentFactory;
    }

    public async Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken ct)
    {
        const string sql = """
        SELECT id_department, name, code
        FROM `department`
        ORDER BY id_department DESC
        """;

        var list = new List<Department>();

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
            list.Add(Map(reader));

        return list;
    }

    public async Task<Department?> GetByIdAsync(int idDepartment, CancellationToken ct)
    {
        const string sql = """
        SELECT id_department, name, code
        FROM `department`
        WHERE id_department = @idDepartment
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idDepartment", idDepartment);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    private Department Map(DbDataReader reader) =>
        _departmentFactory.Create(
            Convert.ToInt32(reader["id_department"]),
            Convert.ToString(reader["name"]) ?? string.Empty,
            Convert.ToInt32(reader["code"]));

    private static void AddParameter(DbCommand cmd, string name, object? value)
    {
        var param = cmd.CreateParameter();
        param.ParameterName = name;
        param.Value = value ?? DBNull.Value;
        cmd.Parameters.Add(param);
    }
}
