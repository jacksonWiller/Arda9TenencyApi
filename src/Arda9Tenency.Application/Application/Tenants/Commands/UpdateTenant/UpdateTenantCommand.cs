using Ardalis.Result;
using MediatR;

namespace Arda9Tenant.Api.Application.Tenants.Commands.UpdateTenant;

public class UpdateTenantCommand : IRequest<Result<UpdateTenantResponse>>
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? Status { get; set; } // active, inactive, suspended
    public string? Plan { get; set; } // basic, pro, enterprise
}
