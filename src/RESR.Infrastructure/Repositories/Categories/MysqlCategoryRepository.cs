
using System.Data.Common;
using MySql.Data.MySqlClient;
using RESR.Core.Controllers.Categories.Ports;
using RESR.Models.Categories;

namespace RESR.Infrastructure.Categories;

public sealed class MySqlCategoryRepository : ICategoryRepository
{
    private readonly Func<DbConnection> _connectionFactory;

    public MySqlCategoryRepository(string connectionString)
    {
        _connectionFactory = () => new MySqlConnection(connectionString);
    }

    internal MySqlCategoryRepository(Func<DbConnection> connectionFactory)
    {
        _connectionFactory = connectionFactory;
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

    private Category Map(DbDataReader reader) =>
        new Category
        {
            IdCategory = reader.GetInt32(reader.GetOrdinal("id_category")),
            Name = reader.GetString(reader.GetOrdinal("name"))
        };


}
