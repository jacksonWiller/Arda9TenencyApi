using Ardalis.Result;
using MediatR;
using Arda9Template.Api.Models;

namespace Arda9Template.Api.Application.Folders.Queries.GetFolderById;

public class GetFolderByIdQuery : IRequest<Result<FolderModel>>
{
    public Guid TenantId { get; set; }
    public Guid FolderId { get; set; }
}