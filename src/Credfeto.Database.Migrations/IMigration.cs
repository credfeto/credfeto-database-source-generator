namespace Credfeto.Database.Migrations;

public interface IMigration
{
    long Id { get; }

    string Name { get; }

    string Sql { get; }
}
