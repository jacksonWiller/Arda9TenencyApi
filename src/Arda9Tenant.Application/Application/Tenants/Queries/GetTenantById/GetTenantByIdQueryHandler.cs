using Arda9Tenant.Domain.Repositories;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Arda9Tenant.Application.Application.Tenants.Queries.GetTenantById;

public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, Result<GetTenantByIdResponse>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<GetTenantByIdQueryHandler> _logger;

    public GetTenantByIdQueryHandler(
        ITenantRepository tenantRepository,
        ILogger<GetTenantByIdQueryHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _logger = logger;
    }

    public async Task<Result<GetTenantByIdResponse>> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await _tenantRepository.GetByIdAsync(request.Id);

            if (tenant == null)
            {
                return Result<GetTenantByIdResponse>.Error("Tenant não encontrado");
            }

            var response = new GetTenantByIdResponse
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Domain = tenant.Domain,
                TenantMaster = tenant.TenantMaster,
                LogoIcon = tenant.LogoIcon,
                LogoFull = tenant.LogoFull,
                PrimaryColor = tenant.PrimaryColor,
                SecondaryColor = tenant.SecondaryColor,
                Status = tenant.Status,
                Plan = tenant.Plan,
                ClientsCount = tenant.ClientsCount,
                UsersCount = tenant.UsersCount,
                CreatedAt = tenant.CreatedAt,
                UpdatedAt = tenant.UpdatedAt
            };

            return Result<GetTenantByIdResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar tenant: {TenantId}", request.Id);
            return Result<GetTenantByIdResponse>.Error("Erro ao buscar tenant");
        }
    }
}
