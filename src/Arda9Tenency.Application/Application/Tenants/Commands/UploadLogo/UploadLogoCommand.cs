using Ardalis.Result;
using MediatR;

namespace Arda9Template.Api.Application.Tenants.Commands.UploadLogo;

public class UploadLogoCommand : IRequest<Result<UploadLogoResponse>>
{
    public Guid TenantId { get; set; }
    public string LogoUrl { get; set; } = string.Empty;
}
