using FluentValidation;

namespace Arda9Tenant.Api.Application.Tenants.Commands.UploadLogo;

public class UploadLogoCommandValidator : AbstractValidator<UploadLogoCommand>
{
    public UploadLogoCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("ID do tenant é obrigatório");

        RuleFor(x => x.LogoUrl)
            .NotEmpty().WithMessage("URL do logo é obrigatória")
            .Must(BeAValidUrl).WithMessage("URL do logo deve ser uma URL válida (HTTP ou HTTPS)")
            .MaximumLength(2048).WithMessage("URL do logo não pode exceder 2048 caracteres");
    }

    private bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
