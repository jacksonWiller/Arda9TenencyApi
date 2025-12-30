using Ardalis.Result;
using MediatR;

namespace Arda9Tenant.Api.Application.Tenants.Queries.GetAllTenants;

public class GetAllTenantsQuery : IRequest<Result<GetAllTenantsResponse>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? Status { get; set; }
}
