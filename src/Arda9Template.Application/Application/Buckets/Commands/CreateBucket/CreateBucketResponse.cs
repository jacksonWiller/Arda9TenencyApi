using Arda9Template.Api.Models;

namespace Arda9Template.Api.Application.Buckets.Commands.CreateBucket;

public class CreateBucketResponse
{
    public BucketModel? Bucket { get; set; }
    public string Message { get; set; } = string.Empty;
}