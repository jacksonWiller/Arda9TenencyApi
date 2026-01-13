using Arda9Tenant.Domain.Repositories;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Arda9Tenant.Application.Application.Tenants.Queries.GetAllTenants;

public class GetAllTenantsQueryHandler : IRequestHandler<GetAllTenantsQuery, Result<GetAllTenantsResponse>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<GetAllTenantsQueryHandler> _logger;

    public GetAllTenantsQueryHandler(
        ITenantRepository tenantRepository,
        ILogger<GetAllTenantsQueryHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _logger = logger;
    }

    public async Task<Result<GetAllTenantsResponse>> Handle(GetAllTenantsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (tenants, total) = await _tenantRepository.GetAllAsync(
                request.Page,
                request.PageSize,
                request.Search,
                request.Status
            );

            var tenantDtos = tenants.Select(t => new TenantDto
            {
                Id = t.Id,
                Name = t.Name,
                Domain = t.Domain,
                TenantMaster = t.TenantMaster,
                LogoIcon = t.LogoIcon,
                LogoFull = t.LogoFull,
                PrimaryColor = t.PrimaryColor,
                SecondaryColor = t.SecondaryColor,
                Status = t.Status,
                Plan = t.Plan,
                ClientsCount = t.ClientsCount,
                UsersCount = t.UsersCount,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            }).ToList();

            var response = new GetAllTenantsResponse
            {
                Data = tenantDtos,
                Total = total,
                Page = request.Page,
                PageSize = request.PageSize
            };

            return Result<GetAllTenantsResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar tenants");
            return Result<GetAllTenantsResponse>.Error("Erro ao buscar tenants");
        }
    }
}
