using FluentValidation;

namespace Arda9Tenant.Api.Application.Tenants.Commands.CreateTenant;

public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(200).WithMessage("Nome não pode ter mais de 200 caracteres");

        RuleFor(x => x.Domain)
            .NotEmpty().WithMessage("Domínio é obrigatório")
            .MaximumLength(100).WithMessage("Domínio não pode ter mais de 100 caracteres")
            .Matches(@"^[a-z0-9\-\.]+$").WithMessage("Domínio contém caracteres inválidos");

        RuleFor(x => x.Plan)
            .NotEmpty().WithMessage("Plano é obrigatório")
            .Must(plan => new[] { "basic", "pro", "enterprise" }.Contains(plan))
            .WithMessage("Plano deve ser: basic, pro ou enterprise");

        RuleFor(x => x.TenantMasterId)
            .NotEmpty().WithMessage("TenantMasterId é obrigatório");

        RuleFor(x => x.PrimaryColor)
            .Matches(@"^#[0-9A-Fa-f]{6}$")
            .When(x => !string.IsNullOrEmpty(x.PrimaryColor))
            .WithMessage("Cor primária deve estar no formato hexadecimal (#RRGGBB)");

        RuleFor(x => x.SecondaryColor)
            .Matches(@"^#[0-9A-Fa-f]{6}$")
            .When(x => !string.IsNullOrEmpty(x.SecondaryColor))
            .WithMessage("Cor secundária deve estar no formato hexadecimal (#RRGGBB)");
    }
}
