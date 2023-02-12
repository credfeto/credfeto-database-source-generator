namespace Credfeto.Database.Source.Generation.Tests;

internal static class Constants
{
    public const string DatabaseTypes = @"namespace Credfeto.Database.Interfaces
    {
        public enum SqlObjectType
        {
            SCALAR_FUNCTION,
            TABLE_FUNCTION,
            STORED_PROCEDURE
        }

        [AttributeUsage(AttributeTargets.Method)]
        public sealed class SqlObjectMapAttribute : Attribute
        {
            public SqlObjectMapAttribute(string name, SqlObjectType sqlObjectType)
            {
                this.Name = name;
                this.SqlObjectType = sqlObjectType;
            }

            public string Name { get; }

            public SqlObjectType SqlObjectType { get; }
        }
    }";
}