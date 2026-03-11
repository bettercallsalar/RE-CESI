using RESR.Core.Controllers.Permissions.Factories;
using RESR.Core.Controllers.Roles.Factories;
using RESR.Core.Controllers.Roles.Ports;
using RESR.Models.Permissions;
using RESR.Models.Roles;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace RESR.Infrastructure.Roles;

public sealed class MySqlRoleRepository : IRoleRepository
{
    private readonly Func<DbConnection> _connectionFactory;
    private readonly IRoleFactory _roleFactory;
    private readonly IPermissionFactory _permissionFactory;

    public MySqlRoleRepository(
        string connectionString,
        IRoleFactory roleFactory,
        IPermissionFactory permissionFactory
    )
    {
        _connectionFactory = () => new MySqlConnection(connectionString);
        _roleFactory = roleFactory;
        _permissionFactory = permissionFactory;
    }

    internal MySqlRoleRepository(
        Func<DbConnection> connectionFactory,
        IRoleFactory roleFactory,
        IPermissionFactory permissionFactory
    )
    {
        _connectionFactory = connectionFactory;
        _roleFactory = roleFactory;
        _permissionFactory = permissionFactory;
    }

    public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken ct)
    {
        const string sql = """
        SELECT id_role, name, description
        FROM `role`
        ORDER BY id_role DESC
        """;

        var list = new List<Role>();

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
            list.Add(MapRole(reader));

        return list;
    }

    public async Task<Role?> GetByIdAsync(int idRole, CancellationToken ct)
    {
        const string sql = """
        SELECT id_role, name, description
        FROM `role`
        WHERE id_role = @idRole
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idRole", idRole);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? MapRole(reader) : null;
    }

    public async Task<IReadOnlyList<Permission>> GetPermissionsByRoleIdAsync(int idRole, CancellationToken ct)
    {
        const string sql = """
        SELECT p.id_permission, p.name, p.description
        FROM `role_permission` rp
        INNER JOIN `permission` p ON p.id_permission = rp.id_permission
        WHERE rp.id_role = @idRole
        ORDER BY p.id_permission
        """;

        var list = new List<Permission>();

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idRole", idRole);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            list.Add(MapPermission(reader));

        return list;
    }

    public async Task<bool> AddPermissionToRoleAsync(int idRole, int idPermission, CancellationToken ct)
    {
        const string sql = """
        INSERT IGNORE INTO `role_permission` (`id_role`, `id_permission`)
        VALUES (@idRole, @idPermission)
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idRole", idRole);
        AddParameter(cmd, "@idPermission", idPermission);

        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }

    public async Task<bool> RemovePermissionFromRoleAsync(int idRole, int idPermission, CancellationToken ct)
    {
        const string sql = """
        DELETE FROM `role_permission`
        WHERE id_role = @idRole AND id_permission = @idPermission
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idRole", idRole);
        AddParameter(cmd, "@idPermission", idPermission);

        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }

    private Role MapRole(DbDataReader reader) =>
        _roleFactory.Create(
            Convert.ToInt32(reader["id_role"]),
            Convert.ToString(reader["name"]) ?? string.Empty,
            reader["description"] == DBNull.Value ? null : Convert.ToString(reader["description"])
        );

    private Permission MapPermission(DbDataReader reader) =>
        _permissionFactory.Create(
            Convert.ToInt32(reader["id_permission"]),
            Convert.ToString(reader["name"]) ?? string.Empty,
            reader["description"] == DBNull.Value ? null : Convert.ToString(reader["description"])
        );

    private static void AddParameter(DbCommand cmd, string name, object? value)
    {
        var param = cmd.CreateParameter();
        param.ParameterName = name;
        param.Value = value ?? DBNull.Value;
        cmd.Parameters.Add(param);
    }
}
