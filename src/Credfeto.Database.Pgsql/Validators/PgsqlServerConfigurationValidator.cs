using FluentValidation;

namespace Credfeto.Database.Pgsql.Validators;

public sealed class PgsqlServerConfigurationValidator : AbstractValidator<PgsqlServerConfiguration>
{
    public PgsqlServerConfigurationValidator()
    {
        this.RuleFor(x => x.ConnectionString)
            .NotEmpty()
            .SetValidator(new PostgresqlConnectionStringValidator());
    }
}