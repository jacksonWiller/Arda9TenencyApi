using Arda9Template.Api.Models;
using Arda9Template.Domain.Repositories;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Arda9Template.Api.Application.Tenants.Commands.CreateTenant;

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

            var tenant = new TenantModel
            {
                Name = request.Name,
                Domain = request.Domain,
                PrimaryColor = request.PrimaryColor ?? "#0066cc",
                SecondaryColor = request.SecondaryColor ?? "#4d94ff",
                Plan = request.Plan,
                Status = "active"
            };

            await _tenantRepository.CreateAsync(tenant);

            _logger.LogInformation("Tenant criado: {TenantId} - {TenantName}", tenant.Id, tenant.Name);

            var response = new CreateTenantResponse
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Domain = tenant.Domain,
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
