using RESR.Core.Roles.Ports;
using RESR.Models.Roles;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace RESR.Infrastructure.Roles;

public sealed class MySqlRoleRepository : IRoleRepository
{
    private readonly string _cs;
    public MySqlRoleRepository(string connectionString) => _cs = connectionString;

    public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken ct)
    {
        const string sql = """
        SELECT id_role, name
        FROM `role`
        ORDER BY id_role DESC
        """;

        var list = new List<Role>();

        await using var conn = new MySqlConnection(_cs);
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, conn);
        await using var r = await cmd.ExecuteReaderAsync(ct);

        while (await r.ReadAsync(ct))
            list.Add(Map(r));

        return list;
    }

    private static Role Map(DbDataReader r)
    {
        return new Role
        {
            IdRole = Convert.ToInt32(r["id_role"]),
            Name = Convert.ToString(r["name"]) ?? ""
        };
    }
}