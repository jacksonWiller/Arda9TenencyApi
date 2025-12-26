using Arda9FileApi.Application.Files.Commands.MoveFile;
using Ardalis.Result;
using MediatR;

namespace Arda9Template.Api.Application.Files.Commands.MoveFile;

public class MoveFileCommand : IRequest<Result<MoveFileResponse>>
{
    public Guid TenantId { get; set; }
    public Guid FileId { get; set; }
    public Guid? FolderId { get; set; }
}
