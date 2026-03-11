using System.Data;
using System.Data.Common;

namespace RESR.Infrastructure.Tests.DbFakes;

internal sealed class FakeDbCommand : DbCommand
{
    private readonly FakeDbParameterCollection _parameters = new();

    public Func<FakeDbCommand, DbDataReader>? ExecuteReaderHandler { get; set; }
    public Func<FakeDbCommand, int>? ExecuteNonQueryHandler { get; set; }
    public Func<FakeDbCommand, object?>? ExecuteScalarHandler { get; set; }

    [System.Diagnostics.CodeAnalysis.AllowNull]
    public override string CommandText { get; set; } = string.Empty;
    public override int CommandTimeout { get; set; }
    public override CommandType CommandType { get; set; } = CommandType.Text;
    public override bool DesignTimeVisible { get; set; }
    public override UpdateRowSource UpdatedRowSource { get; set; } = UpdateRowSource.None;

    protected override DbConnection? DbConnection { get; set; }
    protected override DbParameterCollection DbParameterCollection => _parameters;
    protected override DbTransaction? DbTransaction { get; set; }

    public override void Prepare()
    {
    }

    public override void Cancel()
    {
    }

    public override int ExecuteNonQuery() => ExecuteNonQueryHandler?.Invoke(this) ?? 0;

    public override object? ExecuteScalar() => ExecuteScalarHandler?.Invoke(this);

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) =>
        ExecuteReaderHandler?.Invoke(this) ?? new DataTable().CreateDataReader();

    public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken) =>
        Task.FromResult(ExecuteNonQuery());

    public override Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken) =>
        Task.FromResult(ExecuteScalar());

    protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken) =>
        Task.FromResult(ExecuteDbDataReader(behavior));

    protected override DbParameter CreateDbParameter() => new FakeDbParameter();
}
