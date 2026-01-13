using Ardalis.Result;
using MediatR;

namespace Arda9Tenant.Application.Application.Tenants.Commands.DeleteTenant;

public class DeleteTenantCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
}
