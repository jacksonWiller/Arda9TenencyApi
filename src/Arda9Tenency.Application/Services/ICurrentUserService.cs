namespace Arda9Tenant.Api.Services;

public interface ICurrentUserService
{
    Guid GetTenantId();
    string? GetUserId();
    string? GetUserEmail();
    bool IsAuthenticated();
}