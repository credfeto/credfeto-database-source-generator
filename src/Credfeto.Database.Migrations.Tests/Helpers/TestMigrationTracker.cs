namespace Credfeto.Database.Migrations.Tests.Helpers;

internal sealed class TestMigrationTracker : MigrationTrackerBase
{
    public TestMigrationTracker(MigrationRunnerOptions? options = null)
        : base(options) { }

    public string ConfiguredTableName => this.TableName;

    protected override string BuildEnsureCreatedSql()
    {
        return "ENSURE " + this.TableName;
    }

    protected override string BuildSelectAppliedIdsSql()
    {
        return "SELECT " + this.TableName;
    }

    protected override string BuildInsertAppliedSql()
    {
        return "INSERT " + this.TableName;
    }
}
