using Ardalis.Result;
using MediatR;

namespace Arda9Template.Api.Application.Files.Queries.DownloadFile;

public class DownloadFileQuery : IRequest<Result<DownloadFileResponse>>
{
    public Guid TenantId { get; set; }
    public Guid FileId { get; set; }
}