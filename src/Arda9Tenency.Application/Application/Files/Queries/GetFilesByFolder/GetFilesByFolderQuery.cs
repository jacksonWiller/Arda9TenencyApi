using Ardalis.Result;
using MediatR;
using Arda9Template.Api.Models;

namespace Arda9Template.Api.Application.Files.Queries.GetFilesByFolder;

public class GetFilesByFolderQuery : IRequest<Result<List<FileMetadataModel>>>
{
    public Guid TenantId { get; set; }
    public Guid FolderId { get; set; }
}