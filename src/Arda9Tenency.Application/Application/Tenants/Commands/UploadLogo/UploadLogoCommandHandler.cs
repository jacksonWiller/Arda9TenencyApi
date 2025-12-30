using Arda9Tenant.Domain.Repositories;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Arda9Tenant.Api.Application.Tenants.Commands.UploadLogo;

public class UploadLogoCommandHandler : IRequestHandler<UploadLogoCommand, Result<UploadLogoResponse>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<UploadLogoCommandHandler> _logger;

    public UploadLogoCommandHandler(
        ITenantRepository tenantRepository,
        ILogger<UploadLogoCommandHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _logger = logger;
    }

    public async Task<Result<UploadLogoResponse>> Handle(UploadLogoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.LogoUrl))
            {
                return Result<UploadLogoResponse>.Invalid(new List<ValidationError>
                {
                    new ValidationError
                    {
                        Identifier = nameof(request.LogoUrl),
                        ErrorMessage = "URL do logo não fornecida"
                    }
                });
            }

            // Validar se é uma URL válida
            if (!Uri.TryCreate(request.LogoUrl, UriKind.Absolute, out var uriResult)
                || (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                return Result<UploadLogoResponse>.Invalid(new List<ValidationError>
                {
                    new ValidationError
                    {
                        Identifier = nameof(request.LogoUrl),
                        ErrorMessage = "URL do logo inválida"
                    }
                });
            }

            var tenant = await _tenantRepository.GetByIdAsync(request.TenantId);

            if (tenant == null)
            {
                return Result<UploadLogoResponse>.NotFound("Tenant não encontrado");
            }

            // Atualizar tenant com a URL do logo
            tenant.Logo = request.LogoUrl;
            await _tenantRepository.UpdateAsync(tenant);

            _logger.LogInformation("Logo atualizado para tenant: {TenantId} com URL: {LogoUrl}",
                request.TenantId, request.LogoUrl);

            var response = new UploadLogoResponse
            {
                TenantId = tenant.Id,
                LogoUrl = tenant.Logo,
                UpdatedAt = tenant.UpdatedAt
            };

            return Result<UploadLogoResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar logo do tenant: {TenantId}", request.TenantId);
            return Result<UploadLogoResponse>.Error("Erro ao atualizar logo do tenant");
        }
    }
}
