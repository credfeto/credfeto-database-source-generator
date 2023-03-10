using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Credfeto.Database.SqlServer;

[DebuggerDisplay("{ConnectionString}")]
public sealed class SqlServerConfiguration
{
    [JsonConstructor]
    public SqlServerConfiguration(string connectionString)
    {
        this.ConnectionString = connectionString;
    }

    public string ConnectionString { get; }
}