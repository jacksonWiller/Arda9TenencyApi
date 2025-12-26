using Core.Results;
using MediatR;

namespace Arda9Template.Api.Application.Tenants.Commands.DeleteTenant;

public class DeleteTenantCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
}
