using Arda9Tenant.Api.Models;

namespace Arda9Tenant.Api.Application.Buckets.Commands.CreateBucket;

public class CreateBucketResponse
{
    public BucketModel? Bucket { get; set; }
    public string Message { get; set; } = string.Empty;
}