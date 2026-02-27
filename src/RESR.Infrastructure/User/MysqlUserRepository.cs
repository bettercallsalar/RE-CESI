using MySql.Data.MySqlClient;
using RESR.Core.Controllers.Users;
using RESR.Core.Controllers.Users.Ports;
using RESR.Models.Users;
using System.Data.Common;

namespace RESR.Infrastructure.Users;

public sealed class MySqlUserRepository : IUserRepository
{
    private readonly string _cs;
    public MySqlUserRepository(string connectionString) => _cs = connectionString;

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

        await using var r = await cmd.ExecuteReaderAsync(ct);
        return await r.ReadAsync(ct) ? Map(r) : null;
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

        await using var r = await cmd.ExecuteReaderAsync(ct);
        return await r.ReadAsync(ct) ? Map(r) : null;
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

        await using var r = await cmd.ExecuteReaderAsync(ct);
        return await r.ReadAsync(ct) ? Map(r) : null;
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct)
    {
        const string sql = """
        SELECT id_user, username, first_name, birth_date, bio, email, hashed_password, is_verified, deleted_at, id_department, id_role
        FROM `user`
        WHERE deleted_at IS NULL
        ORDER BY id_user DESC
        """;

        var list = new List<User>();

        await using var conn = new MySqlConnection(_cs);
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, conn);
        await using var r = await cmd.ExecuteReaderAsync(ct);

        while (await r.ReadAsync(ct))
            list.Add(Map(r));

        return list;
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
        cmd.Parameters.AddWithValue("@first_name", (object?)user.FirstName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@birth_date", user.BirthDate is null ? DBNull.Value : user.BirthDate.Value.ToDateTime(TimeOnly.MinValue));
        cmd.Parameters.AddWithValue("@bio", (object?)user.Bio ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@email", user.Email);
        cmd.Parameters.AddWithValue("@hashed_password", user.HashedPassword);
        cmd.Parameters.AddWithValue("@is_verified", user.IsVerified);
        cmd.Parameters.AddWithValue("@deleted_at", (object?)user.DeletedAt ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@id_department", (object?)user.IdDepartment ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@id_role", (object?)user.IdRole ?? DBNull.Value);

        var id = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(id);
    }

    public async Task<User> PatchAsync(UpdateUserCommand user, CancellationToken ct)
    {
        const string sql = """
        UPDATE `user`
        SET
            username = COALESCE(@username, username),
            first_name = COALESCE(@first_name, first_name),
            birth_date = COALESCE(@birth_date, birth_date),
            bio = COALESCE(@bio, bio),
            email = COALESCE(@email, email),
            is_verified = COALESCE(@is_verified, is_verified),
            id_department = COALESCE(@id_department, id_department),
            id_role = COALESCE(@id_role, id_role)
        WHERE id_user = @id_user AND deleted_at IS NULL;
        SELECT id_user, username, first_name, birth_date, bio, email, hashed_password, is_verified, deleted_at, id_department, id_role
        FROM `user`
        WHERE id_user = @id_user;
        """;

        await using var conn = new MySqlConnection(_cs);
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id_user", user.IdUser);
        cmd.Parameters.AddWithValue("@username", (object?)user.Username ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@first_name", (object?)user.FirstName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@birth_date", user.BirthDate is null ? DBNull.Value : user.BirthDate.Value.ToDateTime(TimeOnly.MinValue));
        cmd.Parameters.AddWithValue("@bio", (object?)user.Bio ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@email", (object?)user.Email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@is_verified", (object?)user.IsVerified ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@id_department", (object?)user.IdDepartment ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@id_role", (object?)user.IdRole ?? DBNull.Value);

        await using var r = await cmd.ExecuteReaderAsync(ct);
        return await r.ReadAsync(ct) ? Map(r) : throw new InvalidOperationException("User not found");
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

    private static User Map(DbDataReader r)
    {
        DateOnly? birthDate = null;
        if (r["birth_date"] != DBNull.Value)
            birthDate = DateOnly.FromDateTime(Convert.ToDateTime(r["birth_date"]));

        return new User
        {
            IdUser = Convert.ToInt32(r["id_user"]),
            Username = Convert.ToString(r["username"]) ?? "",
            FirstName = r["first_name"] == DBNull.Value ? null : Convert.ToString(r["first_name"]),
            BirthDate = birthDate,
            Bio = r["bio"] == DBNull.Value ? null : Convert.ToString(r["bio"]),
            Email = Convert.ToString(r["email"]) ?? "",
            HashedPassword = Convert.ToString(r["hashed_password"]) ?? "",
            IsVerified = Convert.ToBoolean(r["is_verified"]),
            DeletedAt = r["deleted_at"] == DBNull.Value ? null : Convert.ToDateTime(r["deleted_at"]),
            IdDepartment = r["id_department"] == DBNull.Value ? null : Convert.ToInt32(r["id_department"]),
            IdRole = r["id_role"] == DBNull.Value ? null : Convert.ToInt32(r["id_role"])
        };
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

        await using var r = await cmd.ExecuteReaderAsync(ct);
        return await r.ReadAsync(ct) ? Map(r) : null;
    }
}
