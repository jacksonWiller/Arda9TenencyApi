using Ardalis.Result;
using MediatR;
using Arda9Template.Api.Models;

namespace Arda9FileApi.Application.Files.Queries.GetRootFiles;

public class GetRootFilesQuery : IRequest<Result<List<FileMetadataModel>>>
{
    public Guid TenantId { get; set; }
    public Guid BucketId { get; set; }
}
