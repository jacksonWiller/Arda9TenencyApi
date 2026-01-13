using Ardalis.Result;
using MediatR;

namespace Arda9Tenant.Application.Application.Tenants.Commands.UploadLogo;

public class UploadLogoCommand : IRequest<Result<UploadLogoResponse>>
{
    public Guid TenantId { get; set; }
    public string? LogoIconUrl { get; set; }
    public string? LogoFullUrl { get; set; }
}
