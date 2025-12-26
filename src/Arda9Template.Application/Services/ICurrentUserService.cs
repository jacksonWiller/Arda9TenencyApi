namespace Arda9Template.Api.Services;

public interface ICurrentUserService
{
    Guid GetTenantId();
    string? GetUserId();
    string? GetUserEmail();
    bool IsAuthenticated();
}