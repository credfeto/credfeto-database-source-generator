using FluentValidation;

namespace Credfeto.Database.SqlServer.Validators;

public sealed class SqlServerConfigurationValidator : AbstractValidator<SqlServerConfiguration>
{
    public SqlServerConfigurationValidator()
    {
        this.RuleFor(x => x.ConnectionString)
            .NotEmpty()
            .SetValidator(new SqlConnectionStringValidator());
    }
}