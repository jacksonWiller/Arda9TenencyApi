namespace Arda9Tenant.Application.Services;

public interface ICurrentUserService
{
    Guid GetTenantId();
    string? GetUserId();
    string? GetUserEmail();
    bool IsAuthenticated();
}