using System;

namespace Credfeto.Database.Migrations;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class DatabaseMigrationsAttribute : Attribute;
