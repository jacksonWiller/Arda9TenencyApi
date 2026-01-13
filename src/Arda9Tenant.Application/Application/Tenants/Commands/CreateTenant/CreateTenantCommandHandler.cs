using Arda9Tenant.Application.Services;
using Arda9Tenant.Domain.Models;
using Arda9Tenant.Domain.Repositories;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Arda9Tenant.Application.Application.Tenants.Commands.CreateTenant;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<CreateTenantResponse>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateTenantCommandHandler> _logger;

    public CreateTenantCommandHandler(
        ITenantRepository tenantRepository,
        ICurrentUserService currentUserService,
        ILogger<CreateTenantCommandHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<CreateTenantResponse>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Obter ID do usuário autenticado
            var userId = _currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UserId not found in token");
                return Result<CreateTenantResponse>.Forbidden();
            }

            // Converter userId para Guid
            if (!Guid.TryParse(userId, out var userGuid))
            {
                _logger.LogWarning("UserId is not a valid GUID: {UserId}", userId);
                return Result<CreateTenantResponse>.Error("Invalid user identifier");
            }

            // Verificar se o TenantMaster existe
            if(request.TenantMasterId.HasValue && request.TenantMasterId.Value != Guid.Empty)
            {
                var tenantMaster = await _tenantRepository.GetByIdAsync(request.TenantMasterId.Value);
                if (tenantMaster == null)
                {
                    _logger.LogWarning("TenantMaster {TenantMasterId} not found", request.TenantMasterId);
                    return Result<CreateTenantResponse>.Error("TenantMaster not found");
                }
            }

            // Validar se o domínio já existe
            if (await _tenantRepository.DomainExistsAsync(request.Domain))
            {
                _logger.LogWarning("Domain {Domain} already exists", request.Domain);
                return Result<CreateTenantResponse>.Error("Domain already exists");
            }

            var tenant = new TenantModel
            {
                Name = request.Name,
                Domain = request.Domain,
                TenantMaster = request.TenantMasterId ?? Guid.Empty,
                CreatedBy = userGuid,
                PrimaryColor = request.PrimaryColor ?? "#0066cc",
                SecondaryColor = request.SecondaryColor ?? "#4d94ff",
                Plan = request.Plan,
                Status = "active"
            };

            await _tenantRepository.CreateAsync(tenant);

            _logger.LogInformation("Tenant created successfully: {TenantId} - {TenantName} - TenantMaster: {TenantMaster}",
                tenant.Id, tenant.Name, tenant.TenantMaster);

            var response = new CreateTenantResponse
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Domain = tenant.Domain,
                TenantMaster = tenant.TenantMaster,
                LogoIcon = tenant.LogoIcon,
                LogoFull = tenant.LogoFull,
                Status = tenant.Status,
                Plan = tenant.Plan,
                CreatedAt = tenant.CreatedAt
            };

            return Result.Success(response, "Tenant created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant: {TenantName}", request.Name);
            return Result<CreateTenantResponse>.Error("Error creating tenant");
        }
    }
}
