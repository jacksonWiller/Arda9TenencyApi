using Ardalis.Result;
using MediatR;

namespace Arda9Tenant.Api.Application.Files.Commands.RestoreFile;

public class RestoreFileCommand : IRequest<Result<RestoreFileResponse>>
{
    public Guid TenantId { get; set; }
    public Guid FileId { get; set; }
}
