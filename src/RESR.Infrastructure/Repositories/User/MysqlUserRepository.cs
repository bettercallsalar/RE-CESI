using MySql.Data.MySqlClient;
using RESR.Core.Controllers.Users;
using RESR.Core.Controllers.Users.Factories;
using RESR.Core.Controllers.Users.Ports;
using RESR.Models.Users;
using System.Data.Common;
using System.Text;

namespace RESR.Infrastructure.Users;

public sealed class MySqlUserRepository : IUserRepository
{
    private readonly string _cs;
    private readonly IUserFactory _userFactory;

    public MySqlUserRepository(string connectionString, IUserFactory userFactory)
    {
        _cs = connectionString;
        _userFactory = userFactory;
    }

    public async Task<User?> GetByIdAsync(int idUser, CancellationToken ct)
    {
        const string sql = """
        SELECT id_user, username, first_name, birth_date, bio, email, hashed_password, is_verified, deleted_at, id_department, id_role
        FROM `user`
        WHERE id_user = @id_user AND deleted_at IS NULL
        """;

        await using var conn = new MySqlConnection(_cs);
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id_user", idUser);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct)
    {
        const string sql = """
        SELECT id_user, username, first_name, birth_date, bio, email, hashed_password, is_verified, deleted_at, id_department, id_role
        FROM `user`
        WHERE email = @email AND deleted_at IS NULL
        """;

        await using var conn = new MySqlConnection(_cs);
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@email", email);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct)
    {
        const string sql = """
        SELECT id_user, username, first_name, birth_date, bio, email, hashed_password, is_verified, deleted_at, id_department, id_role
        FROM `user`
        WHERE username = @username AND deleted_at IS NULL
        """;

        await using var conn = new MySqlConnection(_cs);
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@username", username);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task<IReadOnlyList<User>> GetUsersPaginatedAsync(int page, int pageSize, UserListingFilters filters, CancellationToken ct)
    {
        var offset = (page - 1) * pageSize;
        var sql = new StringBuilder("""
        SELECT id_user, username, first_name, birth_date, bio, email, hashed_password, is_verified, deleted_at, id_department, id_role
        FROM `user`
        WHERE 1 = 1
        """);

        var list = new List<User>();

        await using var conn = new MySqlConnection(_cs);
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand { Connection = conn };
        AppendListingFilters(sql, cmd, filters);
        sql.AppendLine("ORDER BY id_user DESC");
        sql.AppendLine("LIMIT @limit OFFSET @offset");
        cmd.CommandText = sql.ToString();
        cmd.Parameters.AddWithValue("@limit", pageSize);
        cmd.Parameters.AddWithValue("@offset", offset);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
            list.Add(Map(reader));

        return list;
    }

    public async Task<int> CountUsersAsync(UserListingFilters filters, CancellationToken ct)
    {
        var sql = new StringBuilder("""
        SELECT COUNT(*)
        FROM `user`
        WHERE 1 = 1
        """);

        await using var conn = new MySqlConnection(_cs);
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand { Connection = conn };
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

        await using var conn = new MySqlConnection(_cs);
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@username", user.Username);
        cmd.Parameters.AddWithValue("@first_name", user.FirstName);
        cmd.Parameters.AddWithValue("@birth_date", user.BirthDate is null ? DBNull.Value : user.BirthDate.Value.ToDateTime(TimeOnly.MinValue));
        cmd.Parameters.AddWithValue("@bio", (object?)user.Bio ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@email", user.Email);
        cmd.Parameters.AddWithValue("@hashed_password", user.HashedPassword);
        cmd.Parameters.AddWithValue("@is_verified", user.IsVerified);
        cmd.Parameters.AddWithValue("@deleted_at", (object?)user.DeletedAt ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@id_department", user.IdDepartment);
        cmd.Parameters.AddWithValue("@id_role", user.IdRole);

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
        SELECT id_user, username, first_name, birth_date, bio, email, hashed_password, is_verified, deleted_at, id_department, id_role
        FROM `user`
        WHERE id_user = @id_user AND deleted_at IS NULL
        """;

        await using var conn = new MySqlConnection(_cs);
        await conn.OpenAsync(ct);

        await using var updateCmd = new MySqlCommand(updateSql, conn);
        AddPatchParameters(updateCmd, user);

        var rows = await updateCmd.ExecuteNonQueryAsync(ct);
        if (rows == 0)
            throw new InvalidOperationException("User not found");

        await using var selectCmd = new MySqlCommand(selectSql, conn);
        selectCmd.Parameters.AddWithValue("@id_user", user.IdUser);

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

        await using var conn = new MySqlConnection(_cs);
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id_user", idUser);

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
        SELECT id_user, username, first_name, birth_date, bio, email, hashed_password, is_verified, deleted_at, id_department, id_role
        FROM `user`
        WHERE id_user = @id_user AND deleted_at IS NULL
        """;

        await using var conn = new MySqlConnection(_cs);
        await conn.OpenAsync(ct);

        await using var updateCmd = new MySqlCommand(updateSql, conn);
        updateCmd.Parameters.AddWithValue("@id_user", idUser);
        updateCmd.Parameters.AddWithValue("@is_verified", isVerified);

        var rows = await updateCmd.ExecuteNonQueryAsync(ct);
        if (rows == 0)
            throw new InvalidOperationException("User not found");

        await using var selectCmd = new MySqlCommand(selectSql, conn);
        selectCmd.Parameters.AddWithValue("@id_user", idUser);

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
            Convert.ToInt32(reader["id_department"]),
            Convert.ToInt32(reader["id_role"])
        );
    }

    public async Task<User?> GetByEmailAndPasswordHashAsync(string email, string passwordHash, CancellationToken ct)
    {
        const string sql = """
        SELECT id_user, username, first_name, birth_date, bio, email, hashed_password, is_verified, deleted_at, id_department, id_role
        FROM `user`
        WHERE email = @email AND hashed_password = @hashed_password AND deleted_at IS NULL
        """;

        await using var conn = new MySqlConnection(_cs);
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@email", email);
        cmd.Parameters.AddWithValue("@hashed_password", passwordHash);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    private static void AddPatchParameters(MySqlCommand cmd, UpdateUserCommand user)
    {
        cmd.Parameters.AddWithValue("@id_user", user.IdUser);
        cmd.Parameters.AddWithValue("@username", (object?)user.Username ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@first_name", (object?)user.FirstName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@birth_date", user.BirthDate is null ? DBNull.Value : user.BirthDate.Value.ToDateTime(TimeOnly.MinValue));
        cmd.Parameters.AddWithValue("@bio", (object?)user.Bio ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@email", (object?)user.Email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@id_department", (object?)user.IdDepartment ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@id_role", (object?)user.IdRole ?? DBNull.Value);
    }

    private static void AppendListingFilters(StringBuilder sql, MySqlCommand cmd, UserListingFilters filters)
    {
        if (!filters.IncludeDeleted)
            sql.AppendLine("  AND deleted_at IS NULL");

        if (filters.Keyword is not null)
        {
            sql.AppendLine("""
              AND (
                username LIKE @keyword
                OR email LIKE @keyword
                OR first_name LIKE @keyword
                OR bio LIKE @keyword
              )
            """);
            cmd.Parameters.AddWithValue("@keyword", $"%{filters.Keyword}%");
        }

        if (filters.BirthDate is not null)
        {
            sql.AppendLine("  AND birth_date = @birth_date");
            cmd.Parameters.AddWithValue("@birth_date", filters.BirthDate.Value.ToDateTime(TimeOnly.MinValue));
        }

        if (filters.IsVerified is not null)
        {
            sql.AppendLine("  AND is_verified = @is_verified");
            cmd.Parameters.AddWithValue("@is_verified", filters.IsVerified.Value);
        }

        AddIntInFilter(sql, cmd, filters.DepartmentIds, "id_department", "dep");
        AddIntInFilter(sql, cmd, filters.RoleIds, "id_role", "role");
    }

    private static void AddIntInFilter(StringBuilder sql, MySqlCommand cmd, IReadOnlyList<int>? values, string columnName, string parameterPrefix)
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
            cmd.Parameters.AddWithValue(parameterName, values[i]);
        }
        sql.AppendLine(")");
    }
}
