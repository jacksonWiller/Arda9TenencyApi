using Ardalis.Result;
using MediatR;

namespace Arda9Template.Api.Application.Tenants.Queries.GetTenantById;

public class GetTenantByIdQuery : IRequest<Result<GetTenantByIdResponse>>
{
    public Guid Id { get; set; }
}
