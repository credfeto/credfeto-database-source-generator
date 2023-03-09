using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Credfeto.Database.Pgsql;

[DebuggerDisplay("{ConnectionString}")]
public sealed class PgsqlServerConfiguration
{
    [JsonConstructor]
    public PgsqlServerConfiguration(string connectionString)
    {
        this.ConnectionString = connectionString;
    }

    public string ConnectionString { get; }
}