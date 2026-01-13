namespace Arda9Tenant.Application.Application.Tenants.Commands.UploadLogo;

public class UploadLogoResponse
{
    public Guid TenantId { get; set; }
    public string? LogoUrl { get; set; }
    public string? LogoIconUrl { get; set; }
    public string? LogoFullUrl { get; set; }
    public DateTime UpdatedAt { get; set; }
}
