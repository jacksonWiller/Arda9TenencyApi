using Ardalis.Result;
using MediatR;

namespace Arda9Template.Api.Application.Folders.Commands.DeleteFolder;

public class DeleteFolderCommand : IRequest<Result>
{
    public Guid TenantId { get; set; }
    public Guid FolderId { get; set; }
}