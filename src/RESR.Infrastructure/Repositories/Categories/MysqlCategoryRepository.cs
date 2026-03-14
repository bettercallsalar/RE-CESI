
using System.Data.Common;
using MySql.Data.MySqlClient;
using RESR.Core.Controllers.Categories.Factories;
using RESR.Core.Controllers.Categories.Ports;
using RESR.Models.Categories;
using RESR.Core.Controllers.Categories;

namespace RESR.Infrastructure.Categories;

public sealed class MySqlCategoryRepository : ICategoryRepository
{
    private readonly Func<DbConnection> _connectionFactory;
    private readonly ICategoryFactory _categoryFactory;

    public MySqlCategoryRepository(string connectionString, ICategoryFactory categoryFactory)
    {
        _connectionFactory = () => new MySqlConnection(connectionString);
        _categoryFactory = categoryFactory;
    }

    internal MySqlCategoryRepository(Func<DbConnection> connectionFactory, ICategoryFactory categoryFactory)
    {
        _connectionFactory = connectionFactory;
        _categoryFactory = categoryFactory;
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct)
    {
        const string sql = """
        SELECT id_category, name
        FROM `category`
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        var categories = new List<Category>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            categories.Add(Map(reader));
        }

        return categories;
    }

    public async Task<Category?> GetByIdAsync(int idCategory, CancellationToken ct)
    {
        const string sql = """
        SELECT id_category, name
        FROM `category`
        WHERE id_category = @idCategory
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        var idCategoryParam = cmd.CreateParameter();
        idCategoryParam.ParameterName = "@idCategory";
        idCategoryParam.Value = idCategory;
        cmd.Parameters.Add(idCategoryParam);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return Map(reader);
        }

        return null;
    }

    public async Task<IReadOnlyList<Category>> GetFavoriteCategoriesAsync(int idUser, CancellationToken ct)
    {
        const string sql = """
            SELECT c.id_category, c.name
            FROM `user_category` uc
            INNER JOIN `category` c ON c.id_category = uc.id_category
            WHERE uc.id_user = @idUser
            ORDER BY c.id_category DESC
            """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idUser", idUser);

        var categories = new List<Category>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            categories.Add(Map(reader));

        return categories;
    }

    public async Task<AddToUserResult> AddToUserAsync(int idUser, int idCategory, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO `user_category` (id_user, id_category)
            VALUES (@idUser, @idCategory)
            """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idUser", idUser);
        AddParameter(cmd, "@idCategory", idCategory);

        try
        {
            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0 ? AddToUserResult.Added : AddToUserResult.NotFound;
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            return AddToUserResult.AlreadyExists;
        }
        catch (MySqlException ex) when (ex.Number == 1452) // FK constraint
        {
            return AddToUserResult.NotFound;
        }
    }

    public async Task<bool> RemoveFromUserAsync(int idUser, int idCategory, CancellationToken ct)
    {
        const string sql = """
            DELETE FROM `user_category`
            WHERE id_user = @idUser AND id_category = @idCategory
            """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idUser", idUser);
        AddParameter(cmd, "@idCategory", idCategory);

        var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
        return rowsAffected > 0;
    }

    private Category Map(DbDataReader reader) =>
        _categoryFactory.Create(
            reader.GetInt32(reader.GetOrdinal("id_category")),
            reader.GetString(reader.GetOrdinal("name"))
        );

    private static void AddParameter(DbCommand cmd, string name, object? value)
    {
        var parameter = cmd.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        cmd.Parameters.Add(parameter);
    }
}
