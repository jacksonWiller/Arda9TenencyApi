using Ardalis.Result;
using MediatR;
using Arda9Tenant.Api.Models;

namespace Arda9Tenant.Api.Application.Folders.Queries.GetFolderById;

public class GetFolderByIdQuery : IRequest<Result<FolderModel>>
{
    public Guid TenantId { get; set; }
    public Guid FolderId { get; set; }
}