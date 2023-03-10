using System;
using System.Diagnostics;
using FluentValidation;
using FluentValidation.Validators;
using Npgsql;

namespace Credfeto.Database.Pgsql.Validators;

public sealed class PostgresqlConnectionStringValidator : IPropertyValidator<PgsqlServerConfiguration, string>
{
    public bool IsValid(ValidationContext<PgsqlServerConfiguration> context, string value)
    {
        NpgsqlConnectionStringBuilder? cs;

        try
        {
            cs = new(value);
        }
        catch (Exception exception)
        {
            // ignored
            Debug.WriteLine(exception.Message);
            cs = null;
        }

        return cs != null;
    }

    public string GetDefaultMessageTemplate(string errorCode)
    {
        return "Invalid connection string";
    }

    public string Name => "ConnectionString";
}