using RESR.Core.Controllers.Users;
using RESR.Core.Controllers.Users.Factories;
using RESR.Core.Controllers.Users.Ports;
using RESR.Models.Departments;
using RESR.Models.Users;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Text;

namespace RESR.Infrastructure.Users;

public sealed class MySqlUserRepository : IUserRepository
{
    private readonly Func<DbConnection> _connectionFactory;
    private readonly IUserFactory _userFactory;

    public MySqlUserRepository(string connectionString, IUserFactory userFactory)
    {
        _connectionFactory = () => new MySqlConnection(connectionString);
        _userFactory = userFactory;
    }

    internal MySqlUserRepository(Func<DbConnection> connectionFactory, IUserFactory userFactory)
    {
        _connectionFactory = connectionFactory;
        _userFactory = userFactory;
    }

    public async Task<User?> GetByIdAsync(int idUser, CancellationToken ct)
    {
        const string sql = """
        SELECT
            u.id_user,
            u.username,
            u.first_name,
            u.birth_date,
            u.bio,
            u.email,
            u.hashed_password,
            u.is_verified,
            u.deleted_at,
            u.id_department,
            u.id_role,
            d.name AS department_name,
            d.code AS department_code
        FROM `user` u
        INNER JOIN `department` d ON d.id_department = u.id_department
        WHERE u.id_user = @id_user AND u.deleted_at IS NULL
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@id_user", idUser);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct)
    {
        const string sql = """
        SELECT
            u.id_user,
            u.username,
            u.first_name,
            u.birth_date,
            u.bio,
            u.email,
            u.hashed_password,
            u.is_verified,
            u.deleted_at,
            u.id_department,
            u.id_role,
            d.name AS department_name,
            d.code AS department_code
        FROM `user` u
        INNER JOIN `department` d ON d.id_department = u.id_department
        WHERE u.email = @email AND u.deleted_at IS NULL
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@email", email);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct)
    {
        const string sql = """
        SELECT
            u.id_user,
            u.username,
            u.first_name,
            u.birth_date,
            u.bio,
            u.email,
            u.hashed_password,
            u.is_verified,
            u.deleted_at,
            u.id_department,
            u.id_role,
            d.name AS department_name,
            d.code AS department_code
        FROM `user` u
        INNER JOIN `department` d ON d.id_department = u.id_department
        WHERE u.username = @username AND u.deleted_at IS NULL
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@username", username);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task<IReadOnlyList<User>> GetUsersPaginatedAsync(int page, int pageSize, UserListingFilters filters, CancellationToken ct)
    {
        var offset = (page - 1) * pageSize;
        var sql = new StringBuilder("""
        SELECT
            u.id_user,
            u.username,
            u.first_name,
            u.birth_date,
            u.bio,
            u.email,
            u.hashed_password,
            u.is_verified,
            u.deleted_at,
            u.id_department,
            u.id_role,
            d.name AS department_name,
            d.code AS department_code
        FROM `user` u
        INNER JOIN `department` d ON d.id_department = u.id_department
        WHERE 1 = 1
        """);

        var list = new List<User>();

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        AppendListingFilters(sql, cmd, filters);
        sql.AppendLine("ORDER BY id_user DESC");
        sql.AppendLine("LIMIT @limit OFFSET @offset");
        cmd.CommandText = sql.ToString();
        AddParameter(cmd, "@limit", pageSize);
        AddParameter(cmd, "@offset", offset);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
            list.Add(Map(reader));

        return list;
    }

    public async Task<int> CountUsersAsync(UserListingFilters filters, CancellationToken ct)
    {
        var sql = new StringBuilder("""
        SELECT COUNT(*)
        FROM `user` u
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

    public async Task<int> CreateAsync(User user, CancellationToken ct)
    {
        const string sql = """
        INSERT INTO `user` (username, first_name, birth_date, bio, email, hashed_password, is_verified, deleted_at, id_department, id_role)
        VALUES (@username, @first_name, @birth_date, @bio, @email, @hashed_password, @is_verified, @deleted_at, @id_department, @id_role);
        SELECT LAST_INSERT_ID();
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@username", user.Username);
        AddParameter(cmd, "@first_name", user.FirstName);
        AddParameter(cmd, "@birth_date", user.BirthDate is null ? DBNull.Value : user.BirthDate.Value.ToDateTime(TimeOnly.MinValue));
        AddParameter(cmd, "@bio", (object?)user.Bio ?? DBNull.Value);
        AddParameter(cmd, "@email", user.Email);
        AddParameter(cmd, "@hashed_password", user.HashedPassword);
        AddParameter(cmd, "@is_verified", user.IsVerified);
        AddParameter(cmd, "@deleted_at", (object?)user.DeletedAt ?? DBNull.Value);
        AddParameter(cmd, "@id_department", user.Department.IdDepartment);
        AddParameter(cmd, "@id_role", user.IdRole);

        var id = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(id);
    }

    public async Task<User> PatchAsync(UpdateUserCommand user, CancellationToken ct)
    {
        const string updateSql = """
        UPDATE `user`
        SET
            username = COALESCE(@username, username),
            first_name = COALESCE(@first_name, first_name),
            birth_date = COALESCE(@birth_date, birth_date),
            bio = COALESCE(@bio, bio),
            email = COALESCE(@email, email),
            id_department = COALESCE(@id_department, id_department),
            id_role = COALESCE(@id_role, id_role)
        WHERE id_user = @id_user AND deleted_at IS NULL
        """;

        const string selectSql = """
        SELECT
            u.id_user,
            u.username,
            u.first_name,
            u.birth_date,
            u.bio,
            u.email,
            u.hashed_password,
            u.is_verified,
            u.deleted_at,
            u.id_department,
            u.id_role,
            d.name AS department_name,
            d.code AS department_code
        FROM `user` u
        INNER JOIN `department` d ON d.id_department = u.id_department
        WHERE u.id_user = @id_user AND u.deleted_at IS NULL
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var updateCmd = conn.CreateCommand();
        updateCmd.CommandText = updateSql;
        AddPatchParameters(updateCmd, user);

        var rows = await updateCmd.ExecuteNonQueryAsync(ct);
        if (rows == 0)
            throw new InvalidOperationException("User not found");

        await using var selectCmd = conn.CreateCommand();
        selectCmd.CommandText = selectSql;
        AddParameter(selectCmd, "@id_user", user.IdUser);

        await using var reader = await selectCmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct)
            ? Map(reader)
            : throw new InvalidOperationException("User not found");
    }

    public async Task<bool> SoftDeleteAsync(int idUser, CancellationToken ct)
    {
        const string sql = """
        UPDATE `user`
        SET deleted_at = NOW()
        WHERE id_user = @id_user AND deleted_at IS NULL
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@id_user", idUser);

        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }

    public async Task<User> SetVerificationAsync(int idUser, bool isVerified, CancellationToken ct)
    {
        const string updateSql = """
        UPDATE `user`
        SET is_verified = @is_verified
        WHERE id_user = @id_user AND deleted_at IS NULL
        """;

        const string selectSql = """
        SELECT
            u.id_user,
            u.username,
            u.first_name,
            u.birth_date,
            u.bio,
            u.email,
            u.hashed_password,
            u.is_verified,
            u.deleted_at,
            u.id_department,
            u.id_role,
            d.name AS department_name,
            d.code AS department_code
        FROM `user` u
        INNER JOIN `department` d ON d.id_department = u.id_department
        WHERE u.id_user = @id_user AND u.deleted_at IS NULL
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var updateCmd = conn.CreateCommand();
        updateCmd.CommandText = updateSql;
        AddParameter(updateCmd, "@id_user", idUser);
        AddParameter(updateCmd, "@is_verified", isVerified);

        var rows = await updateCmd.ExecuteNonQueryAsync(ct);
        if (rows == 0)
            throw new InvalidOperationException("User not found");

        await using var selectCmd = conn.CreateCommand();
        selectCmd.CommandText = selectSql;
        AddParameter(selectCmd, "@id_user", idUser);

        await using var reader = await selectCmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct)
            ? Map(reader)
            : throw new InvalidOperationException("User not found");
    }

    private User Map(DbDataReader reader)
    {
        DateOnly? birthDate = null;
        if (reader["birth_date"] != DBNull.Value)
            birthDate = DateOnly.FromDateTime(Convert.ToDateTime(reader["birth_date"]));

        if (reader["first_name"] == DBNull.Value)
            throw new InvalidOperationException("User first_name cannot be NULL. Run latest DB migrations.");
        if (reader["id_department"] == DBNull.Value)
            throw new InvalidOperationException("User id_department cannot be NULL. Run latest DB migrations.");
        if (reader["id_role"] == DBNull.Value)
            throw new InvalidOperationException("User id_role cannot be NULL. Run latest DB migrations.");

        return _userFactory.CreateFromPersistence(
            Convert.ToInt32(reader["id_user"]),
            Convert.ToString(reader["username"]) ?? string.Empty,
            Convert.ToString(reader["email"]) ?? string.Empty,
            Convert.ToString(reader["hashed_password"]) ?? string.Empty,
            Convert.ToString(reader["first_name"]) ?? string.Empty,
            birthDate,
            reader["bio"] == DBNull.Value ? null : Convert.ToString(reader["bio"]),
            Convert.ToBoolean(reader["is_verified"]),
            reader["deleted_at"] == DBNull.Value ? null : Convert.ToDateTime(reader["deleted_at"]),
            MapDepartment(reader),
            Convert.ToInt32(reader["id_role"])
        );
    }

    public async Task<User?> GetByEmailAndPasswordHashAsync(string email, string passwordHash, CancellationToken ct)
    {
        const string sql = """
        SELECT
            u.id_user,
            u.username,
            u.first_name,
            u.birth_date,
            u.bio,
            u.email,
            u.hashed_password,
            u.is_verified,
            u.deleted_at,
            u.id_department,
            u.id_role,
            d.name AS department_name,
            d.code AS department_code
        FROM `user` u
        INNER JOIN `department` d ON d.id_department = u.id_department
        WHERE u.email = @email AND u.hashed_password = @hashed_password AND u.deleted_at IS NULL
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@email", email);
        AddParameter(cmd, "@hashed_password", passwordHash);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    private static void AddPatchParameters(DbCommand cmd, UpdateUserCommand user)
    {
        AddParameter(cmd, "@id_user", user.IdUser);
        AddParameter(cmd, "@username", (object?)user.Username ?? DBNull.Value);
        AddParameter(cmd, "@first_name", (object?)user.FirstName ?? DBNull.Value);
        AddParameter(cmd, "@birth_date", user.BirthDate is null ? DBNull.Value : user.BirthDate.Value.ToDateTime(TimeOnly.MinValue));
        AddParameter(cmd, "@bio", (object?)user.Bio ?? DBNull.Value);
        AddParameter(cmd, "@email", (object?)user.Email ?? DBNull.Value);
        AddParameter(cmd, "@id_department", (object?)user.IdDepartment ?? DBNull.Value);
        AddParameter(cmd, "@id_role", (object?)user.IdRole ?? DBNull.Value);
    }

    private static void AppendListingFilters(StringBuilder sql, DbCommand cmd, UserListingFilters filters)
    {
        if (!filters.IncludeDeleted)
            sql.AppendLine("  AND u.deleted_at IS NULL");

        if (filters.Keyword is not null)
        {
            sql.AppendLine("""
              AND (
                u.username LIKE @keyword
                OR u.email LIKE @keyword
                OR u.first_name LIKE @keyword
                OR u.bio LIKE @keyword
              )
            """);
            AddParameter(cmd, "@keyword", $"%{filters.Keyword}%");
        }

        if (filters.BirthDate is not null)
        {
            sql.AppendLine("  AND u.birth_date = @birth_date");
            AddParameter(cmd, "@birth_date", filters.BirthDate.Value.ToDateTime(TimeOnly.MinValue));
        }

        if (filters.IsVerified is not null)
        {
            sql.AppendLine("  AND u.is_verified = @is_verified");
            AddParameter(cmd, "@is_verified", filters.IsVerified.Value);
        }

        AddIntInFilter(sql, cmd, filters.DepartmentIds, "u.id_department", "dep");
        AddIntInFilter(sql, cmd, filters.RoleIds, "u.id_role", "role");
    }

    private static Department MapDepartment(DbDataReader reader)
    {
        if (reader["department_name"] == DBNull.Value)
            throw new InvalidOperationException("User department_name cannot be NULL. Run latest DB migrations.");

        if (reader["department_code"] == DBNull.Value)
            throw new InvalidOperationException("User department_code cannot be NULL. Run latest DB migrations.");

        return new Department
        {
            IdDepartment = Convert.ToInt32(reader["id_department"]),
            Name = Convert.ToString(reader["department_name"]) ?? string.Empty,
            Code = Convert.ToInt32(reader["department_code"])
        };
    }

    private static void AddIntInFilter(StringBuilder sql, DbCommand cmd, IReadOnlyList<int>? values, string columnName, string parameterPrefix)
    {
        if (values is null || values.Count == 0)
            return;

        sql.Append($"  AND {columnName} IN (");
        for (var i = 0; i < values.Count; i++)
        {
            if (i > 0)
                sql.Append(", ");

            var parameterName = $"@{parameterPrefix}{i}";
            sql.Append(parameterName);
            AddParameter(cmd, parameterName, values[i]);
        }
        sql.AppendLine(")");
    }

    private static void AddParameter(DbCommand cmd, string name, object? value)
    {
        var param = cmd.CreateParameter();
        param.ParameterName = name;
        param.Value = value ?? DBNull.Value;
        cmd.Parameters.Add(param);
    }
}
