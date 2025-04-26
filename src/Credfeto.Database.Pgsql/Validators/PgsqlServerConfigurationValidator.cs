using FluentValidation;

namespace Credfeto.Database.Pgsql.Validators;

public sealed class PgsqlServerConfigurationValidator : AbstractValidator<PgsqlServerConfiguration>
{
    public PgsqlServerConfigurationValidator()
    {
        this.RuleFor(static x => x.ConnectionString).NotEmpty().SetValidator(new PostgresqlConnectionStringValidator());
    }
}
