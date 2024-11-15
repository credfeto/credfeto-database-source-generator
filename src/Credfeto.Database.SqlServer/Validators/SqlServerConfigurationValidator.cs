using FluentValidation;

namespace Credfeto.Database.SqlServer.Validators;

public sealed class SqlServerConfigurationValidator : AbstractValidator<SqlServerConfiguration>
{
    public SqlServerConfigurationValidator()
    {
        this.RuleFor(static x => x.ConnectionString)
            .NotEmpty()
            .SetValidator(new SqlConnectionStringValidator());
    }
}