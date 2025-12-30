using Ardalis.Result;
using MediatR;
using Arda9Tenant.Api.Models;

namespace Arda9Tenant.Api.Application.Files.Queries.GetFileById;

public class GetFileByIdQuery : IRequest<Result<FileMetadataModel>>
{
    public Guid TenantId { get; set; }
    public Guid FileId { get; set; }
}