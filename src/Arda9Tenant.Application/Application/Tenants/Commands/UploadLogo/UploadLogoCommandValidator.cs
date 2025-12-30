using FluentValidation;

namespace Arda9Tenant.Api.Application.Tenants.Commands.UploadLogo;

public class UploadLogoCommandValidator : AbstractValidator<UploadLogoCommand>
{
    public UploadLogoCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("ID do tenant é obrigatório");

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.LogoUrl) ||
                       !string.IsNullOrWhiteSpace(x.LogoIconUrl) ||
                       !string.IsNullOrWhiteSpace(x.LogoFullUrl))
            .WithMessage("Pelo menos uma URL de logo deve ser fornecida");

        RuleFor(x => x.LogoUrl)
            .Must(BeAValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.LogoUrl))
            .WithMessage("URL do logo deve ser uma URL válida (HTTP ou HTTPS)")
            .MaximumLength(2048)
            .When(x => !string.IsNullOrWhiteSpace(x.LogoUrl))
            .WithMessage("URL do logo não pode exceder 2048 caracteres");

        RuleFor(x => x.LogoIconUrl)
            .Must(BeAValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.LogoIconUrl))
            .WithMessage("URL do logo ícone deve ser uma URL válida (HTTP ou HTTPS)")
            .MaximumLength(2048)
            .When(x => !string.IsNullOrWhiteSpace(x.LogoIconUrl))
            .WithMessage("URL do logo ícone não pode exceder 2048 caracteres");

        RuleFor(x => x.LogoFullUrl)
            .Must(BeAValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.LogoFullUrl))
            .WithMessage("URL do logo completo deve ser uma URL válida (HTTP ou HTTPS)")
            .MaximumLength(2048)
            .When(x => !string.IsNullOrWhiteSpace(x.LogoFullUrl))
            .WithMessage("URL do logo completo não pode exceder 2048 caracteres");
    }

    private bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
