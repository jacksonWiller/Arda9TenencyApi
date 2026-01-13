using Arda9Tenant.Domain.Models;

namespace Arda9Tenant.Domain.Repositories;

public interface ITenantRepository
{
    Task<TenantModel?> GetByIdAsync(Guid id);
    Task<TenantModel?> GetByDomainAsync(string domain);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> DomainExistsAsync(string domain);
    Task<(List<TenantModel> Tenants, int Total)> GetAllAsync(int page, int pageSize, string? search = null, string? status = null);
    Task CreateAsync(TenantModel tenant);
    Task UpdateAsync(TenantModel tenant);
    Task DeleteAsync(Guid id);
}
