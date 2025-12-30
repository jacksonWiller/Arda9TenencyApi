namespace Arda9Tenant.Api.Application.Tenants.Commands.UploadLogo;

public class UploadLogoResponse
{
    public Guid TenantId { get; set; }
    public string LogoUrl { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
