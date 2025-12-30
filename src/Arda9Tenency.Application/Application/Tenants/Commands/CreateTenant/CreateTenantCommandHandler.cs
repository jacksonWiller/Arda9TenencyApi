using Arda9Tenant.Api.Models;
using Arda9Tenant.Domain.Repositories;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Arda9Tenant.Api.Application.Tenants.Commands.CreateTenant;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<CreateTenantResponse>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<CreateTenantCommandHandler> _logger;

    public CreateTenantCommandHandler(
        ITenantRepository tenantRepository,
        ILogger<CreateTenantCommandHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _logger = logger;
    }

    public async Task<Result<CreateTenantResponse>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validar se o domínio já existe
            if (await _tenantRepository.DomainExistsAsync(request.Domain))
            {
                return Result<CreateTenantResponse>.Error("Domínio já está em uso");
            }

            // Validar se o TenantMasterId foi fornecido
            if (string.IsNullOrWhiteSpace(request.TenantMasterId))
            {
                return Result<CreateTenantResponse>.Error("TenantMasterId é obrigatório");
            }

            var tenant = new TenantModel
            {
                Name = request.Name,
                Domain = request.Domain,
                TenantMaster = request.TenantMasterId,
                PrimaryColor = request.PrimaryColor ?? "#0066cc",
                SecondaryColor = request.SecondaryColor ?? "#4d94ff",
                Plan = request.Plan,
                Status = "active"
            };

            await _tenantRepository.CreateAsync(tenant);

            _logger.LogInformation("Tenant criado: {TenantId} - {TenantName} - TenantMaster: {TenantMaster}", 
                tenant.Id, tenant.Name, tenant.TenantMaster);

            var response = new CreateTenantResponse
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Domain = tenant.Domain,
                TenantMaster = tenant.TenantMaster,
                Logo = tenant.Logo,
                Status = tenant.Status,
                Plan = tenant.Plan,
                CreatedAt = tenant.CreatedAt
            };

            return Result<CreateTenantResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar tenant");
            return Result<CreateTenantResponse>.Error("Erro ao criar tenant");
        }
    }
}
