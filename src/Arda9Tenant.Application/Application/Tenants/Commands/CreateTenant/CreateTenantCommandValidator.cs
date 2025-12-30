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

        RuleFor(x => x.LogoIcon)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.LogoIcon))
            .WithMessage("URL do logo ícone não pode ter mais de 500 caracteres")
            .Must(BeAValidUrl)
            .When(x => !string.IsNullOrEmpty(x.LogoIcon))
            .WithMessage("URL do logo ícone deve ser uma URL válida");

        RuleFor(x => x.LogoFull)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.LogoFull))
            .WithMessage("URL do logo completo não pode ter mais de 500 caracteres")
            .Must(BeAValidUrl)
            .When(x => !string.IsNullOrEmpty(x.LogoFull))
            .WithMessage("URL do logo completo deve ser uma URL válida");
    }

    private bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
