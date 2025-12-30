using Amazon.DynamoDBv2.DataModel;

namespace Arda9Tenant.Api.Models;

/// <summary>
/// DTO para representar uma Tag nas respostas da API e persistência no DynamoDB
/// </summary>
public class TagModel
{
    [DynamoDBProperty]
    public Guid Id { get; set; }

    [DynamoDBProperty]
    public string Name { get; set; } = string.Empty;
}
