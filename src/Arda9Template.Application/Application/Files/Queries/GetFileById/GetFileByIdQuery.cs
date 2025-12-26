using Ardalis.Result;
using MediatR;
using Arda9Template.Api.Models;

namespace Arda9Template.Api.Application.Files.Queries.GetFileById;

public class GetFileByIdQuery : IRequest<Result<FileMetadataModel>>
{
    public Guid TenantId { get; set; }
    public Guid FileId { get; set; }
}