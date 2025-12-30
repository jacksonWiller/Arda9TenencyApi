using Amazon.DynamoDBv2.DataModel;
using Arda9Tenant.Api.Core.Infra;

namespace Arda9Tenant.Api.Models;

/// <summary>
/// DTO for Tenant with DynamoDB single table design
/// PK: TENANT#{TenantId}, SK: METADATA
/// GSI1: DOMAIN#{Domain} -> Buscar tenant por domínio
/// </summary>
[DynamoDBTable("arda9-tenency-v1")]
public class TenantModel : DynamoSingleTableEntity
{
    [DynamoDBIgnore]
    public Guid Id { get; set; }

    [DynamoDBProperty("TenantId")]
    public string TenantId
    {
        get => Id.ToString();
        set => Id = Guid.Parse(value);
    }

    [DynamoDBProperty]
    public string Name { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string TenantMaster { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string Domain { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string? Logo { get; set; }

    [DynamoDBProperty]
    public string PrimaryColor { get; set; } = "#0066cc";

    [DynamoDBProperty]
    public string SecondaryColor { get; set; } = "#4d94ff";

    [DynamoDBProperty]
    public string Status { get; set; } = "active"; // active, inactive, suspended

    [DynamoDBProperty]
    public string Plan { get; set; } = "basic"; // basic, pro, enterprise

    [DynamoDBProperty]
    public DateTime CreatedAt { get; set; }

    [DynamoDBProperty]
    public DateTime UpdatedAt { get; set; }

    [DynamoDBProperty]
    public Guid? CreatedBy { get; set; }

    [DynamoDBProperty]
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Contadores - não persistidos, calculados em runtime
    /// </summary>
    [DynamoDBIgnore]
    public int ClientsCount { get; set; }

    [DynamoDBIgnore]
    public int UsersCount { get; set; }

    // GSI1: Para buscar tenant por domínio
    [DynamoDBGlobalSecondaryIndexHashKey("GSI1-Index", AttributeName = "GSI1PK")]
    public string GSI1PK { get; set; } = string.Empty; // DOMAIN#{Domain}
}
