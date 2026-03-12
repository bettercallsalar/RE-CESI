
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

    public async Task<AddToUserResult> AddToUserAsync(int idCategory, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO `user_category` (id_user, id_category)
            VALUES (@idUser, @idCategory)
            """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        var idUserParam = cmd.CreateParameter();
        idUserParam.ParameterName = "@idUser";
        idUserParam.Value = 1; // TODO: get from context
        cmd.Parameters.Add(idUserParam);

        var idCategoryParam = cmd.CreateParameter();
        idCategoryParam.ParameterName = "@idCategory";
        idCategoryParam.Value = idCategory;
        cmd.Parameters.Add(idCategoryParam);

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

    public async Task<bool> RemoveFromUserAsync(int idCategory, CancellationToken ct)
    {
        const string sql = """
            DELETE FROM `user_category`
            WHERE id_user = @idUser AND id_category = @idCategory
            """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        var idUserParam = cmd.CreateParameter();
        idUserParam.ParameterName = "@idUser";
        idUserParam.Value = 1; // TODO: get from context
        cmd.Parameters.Add(idUserParam);

        var idCategoryParam = cmd.CreateParameter();
        idCategoryParam.ParameterName = "@idCategory";
        idCategoryParam.Value = idCategory;
        cmd.Parameters.Add(idCategoryParam);

        var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
        return rowsAffected > 0;
    }

    private Category Map(DbDataReader reader) =>
        _categoryFactory.Create(
            reader.GetInt32(reader.GetOrdinal("id_category")),
            reader.GetString(reader.GetOrdinal("name"))
        );


}
