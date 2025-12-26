using Ardalis.Result;
using MediatR;
using Arda9Template.Api.Models;

namespace Arda9Template.Api.Application.Folders.Queries.GetFoldersByParent;

public class GetFoldersByParentQuery : IRequest<Result<List<FolderModel>>>
{
    public Guid TenantId { get; set; }
    public Guid ParentFolderId { get; set; }
}