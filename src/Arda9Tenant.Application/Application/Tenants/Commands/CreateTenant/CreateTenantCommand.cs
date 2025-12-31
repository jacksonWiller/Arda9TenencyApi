using Ardalis.Result;
using MediatR;
using System.Text.Json.Serialization;

namespace Arda9Tenant.Api.Application.Tenants.Commands.CreateTenant;

public class CreateTenantCommand : IRequest<Result<CreateTenantResponse>>
{
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string Plan { get; set; } = "basic"; // basic, pro, enterprise
    public Guid? TenantMasterId { get; set; }
}
