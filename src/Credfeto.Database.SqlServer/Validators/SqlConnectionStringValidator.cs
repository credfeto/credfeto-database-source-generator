using System;
using System.Diagnostics;
using FluentValidation;
using FluentValidation.Validators;
using Microsoft.Data.SqlClient;

namespace Credfeto.Database.SqlServer.Validators;

public sealed class SqlConnectionStringValidator
    : IPropertyValidator<SqlServerConfiguration, string>
{
    public bool IsValid(ValidationContext<SqlServerConfiguration> context, string value)
    {
        SqlConnectionStringBuilder? cs;

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

        return cs is not null;
    }

    public string GetDefaultMessageTemplate(string errorCode)
    {
        return "Invalid connection string";
    }

    public string Name => "ConnectionString";
}
