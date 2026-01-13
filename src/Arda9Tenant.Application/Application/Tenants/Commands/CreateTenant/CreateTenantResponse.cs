namespace Arda9Tenant.Application.Application.Tenants.Commands.CreateTenant;

public class CreateTenantResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public Guid TenantMaster { get; set; }
    public string? Logo { get; set; }
    public string? LogoIcon { get; set; }
    public string? LogoFull { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
