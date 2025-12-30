namespace Arda9Tenant.Api.Application.Tenants.Queries.GetAllTenants;

public class GetAllTenantsResponse
{
    public List<TenantDto> Data { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string PrimaryColor { get; set; } = string.Empty;
    public string SecondaryColor { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public int ClientsCount { get; set; }
    public int UsersCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
