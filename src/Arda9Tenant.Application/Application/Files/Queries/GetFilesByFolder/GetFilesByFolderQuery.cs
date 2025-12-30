using Ardalis.Result;
using MediatR;
using Arda9Tenant.Api.Models;

namespace Arda9Tenant.Api.Application.Files.Queries.GetFilesByFolder;

public class GetFilesByFolderQuery : IRequest<Result<List<FileMetadataModel>>>
{
    public Guid TenantId { get; set; }
    public Guid FolderId { get; set; }
}