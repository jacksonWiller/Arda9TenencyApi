using Ardalis.Result;
using MediatR;
using Arda9Tenant.Api.Models;

namespace Arda9Tenant.Api.Application.Folders.Queries.GetFoldersByBucket;

public class GetFoldersByBucketQuery : IRequest<Result<List<FolderModel>>>
{
    public Guid TenantId { get; set; }
    public Guid BucketId { get; set; }
}