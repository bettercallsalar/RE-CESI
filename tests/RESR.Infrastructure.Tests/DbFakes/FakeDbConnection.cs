using System.Data;
using System.Data.Common;

namespace RESR.Infrastructure.Tests.DbFakes;

internal sealed class FakeDbConnection : DbConnection
{
    private readonly Queue<DbCommand> _commands;
    private ConnectionState _state = ConnectionState.Closed;

    public FakeDbConnection(IEnumerable<DbCommand> commands)
    {
        _commands = new Queue<DbCommand>(commands);
    }

    [System.Diagnostics.CodeAnalysis.AllowNull]
    public override string ConnectionString { get; set; } = string.Empty;
    public override string Database => string.Empty;
    public override string DataSource => string.Empty;
    public override string ServerVersion => string.Empty;
    public override ConnectionState State => _state;

    public override void ChangeDatabase(string databaseName)
    {
    }

    public override void Close() => _state = ConnectionState.Closed;

    public override void Open() => _state = ConnectionState.Open;

    public override Task OpenAsync(CancellationToken cancellationToken)
    {
        _state = ConnectionState.Open;
        return Task.CompletedTask;
    }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) =>
        throw new NotSupportedException();

    protected override DbCommand CreateDbCommand() => _commands.Dequeue();
}
