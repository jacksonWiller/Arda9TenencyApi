using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Arda9Template.Api.Application.Tenants.Commands.UploadLogo;

public class UploadLogoCommand : IRequest<Result<UploadLogoResponse>>
{
    public Guid TenantId { get; set; }
    public IFormFile? File { get; set; }
}
