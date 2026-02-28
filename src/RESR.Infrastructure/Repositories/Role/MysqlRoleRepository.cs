using MySql.Data.MySqlClient;
using RESR.Core.Controllers.Permissions.Factories;
using RESR.Core.Controllers.Roles.Factories;
using RESR.Core.Controllers.Roles.Ports;
using RESR.Models.Permissions;
using RESR.Models.Roles;
using System.Data.Common;

namespace RESR.Infrastructure.Roles;

public sealed class MySqlRoleRepository : IRoleRepository
{
    private readonly string _cs;
    private readonly IRoleFactory _roleFactory;
    private readonly IPermissionFactory _permissionFactory;

    public MySqlRoleRepository(
        string connectionString,
        IRoleFactory roleFactory,
        IPermissionFactory permissionFactory
    )
    {
        _cs = connectionString;
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

        await using var conn = new MySqlConnection(_cs);
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, conn);
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

        await using var conn = new MySqlConnection(_cs);
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@idRole", idRole);

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

        await using var conn = new MySqlConnection(_cs);
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@idRole", idRole);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            list.Add(MapPermission(reader));

        return list;
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
}
