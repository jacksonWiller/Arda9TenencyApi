using Arda9Template.Api.Models;

namespace Arda9Template.Api.Application.Buckets.Queries.GetAllBuckets;

public class GetAllBucketsResponse
{
    public List<BucketModel> Buckets { get; set; } = new();
    public string? LastEvaluatedKey { get; set; }
    public int TotalCount { get; set; }
}