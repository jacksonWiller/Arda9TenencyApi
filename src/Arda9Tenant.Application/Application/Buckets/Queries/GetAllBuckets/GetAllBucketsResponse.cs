using Arda9Tenant.Api.Models;

namespace Arda9Tenant.Api.Application.Buckets.Queries.GetAllBuckets;

public class GetAllBucketsResponse
{
    public List<BucketModel> Buckets { get; set; } = new();
    public string? LastEvaluatedKey { get; set; }
    public int TotalCount { get; set; }
}