using System.Diagnostics;
using System.Text.Json.Serialization;
using Npgsql;

namespace Credfeto.Database.Pgsql;

[DebuggerDisplay("{ConnectionString}")]
public sealed class PgsqlServerConfiguration
{
    [JsonConstructor]
    public PgsqlServerConfiguration(string connectionString)
    {
        this.ConnectionString = new NpgsqlConnectionStringBuilder(connectionString) { Multiplexing = true, Pooling = true }.ToString();
    }

    public string ConnectionString { get; }
}