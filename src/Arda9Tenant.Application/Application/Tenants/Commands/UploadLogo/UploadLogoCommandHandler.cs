using Arda9Tenant.Domain.Repositories;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Arda9Tenant.Application.Application.Tenants.Commands.UploadLogo;

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
            var tenant = await _tenantRepository.GetByIdAsync(request.TenantId);

            if (tenant == null)
            {
                _logger.LogWarning("Tenant {TenantId} not found", request.TenantId);
                return Result.NotFound();
            }

            if (!string.IsNullOrWhiteSpace(request.LogoIconUrl))
            {
                tenant.LogoIcon = request.LogoIconUrl;
                _logger.LogInformation("LogoIcon updated for tenant: {TenantId}", request.TenantId);
            }

            if (!string.IsNullOrWhiteSpace(request.LogoFullUrl))
            {
                tenant.LogoFull = request.LogoFullUrl;
                _logger.LogInformation("LogoFull updated for tenant: {TenantId}", request.TenantId);
            }

            await _tenantRepository.UpdateAsync(tenant);

            _logger.LogInformation("Logos updated successfully for tenant: {TenantId}", request.TenantId);

            var response = new UploadLogoResponse
            {
                TenantId = tenant.Id,
                LogoIconUrl = tenant.LogoIcon,
                LogoFullUrl = tenant.LogoFull,
                UpdatedAt = tenant.UpdatedAt
            };

            return Result.Success(response,"Logos updated successfully for tenant");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating logos for tenant: {TenantId}", request.TenantId);
            return Result.Error();
        }
    }
}
