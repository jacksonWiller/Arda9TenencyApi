using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Arda9Template.Domain.Repositories;
using Arda9Template.Api.Models;
using Microsoft.Extensions.Logging;

namespace Arda9Template.Api.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly IDynamoDBContext _context;
    private readonly ILogger<TenantRepository> _logger;

    public TenantRepository(IDynamoDBContext context, ILogger<TenantRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TenantModel?> GetByIdAsync(Guid id)
    {
        try
        {
            var pk = $"TENANT#{id}";
            var sk = "METADATA";
            return await _context.LoadAsync<TenantModel>(pk, sk);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar tenant por ID: {Id}", id);
            throw;
        }
    }

    public async Task<TenantModel?> GetByDomainAsync(string domain)
    {
        try
        {
            // Usar GSI1 para buscar tenant por domínio
            var search = _context.QueryAsync<TenantModel>(
                $"DOMAIN#{domain}",
                new DynamoDBOperationConfig
                {
                    IndexName = "GSI1-Index"
                }
            );

            var results = await search.GetRemainingAsync();
            return results.FirstOrDefault(t => t.EntityType == "TENANT" && t.Status != "deleted");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar tenant por domínio: {Domain}", domain);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        try
        {
            var result = await GetByIdAsync(id);
            return result != null && result.Status != "deleted";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar existência do tenant: {Id}", id);
            throw;
        }
    }

    public async Task<bool> DomainExistsAsync(string domain)
    {
        try
        {
            var result = await GetByDomainAsync(domain);
            return result != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar existência do domínio: {Domain}", domain);
            throw;
        }
    }

    public async Task<(List<TenantModel> Tenants, int Total)> GetAllAsync(int page, int pageSize, string? search = null, string? status = null)
    {
        try
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("EntityType", ScanOperator.Equal, "TENANT"),
                new ScanCondition("Status", ScanOperator.NotEqual, "deleted")
            };

            if (!string.IsNullOrWhiteSpace(status))
            {
                conditions.Add(new ScanCondition("Status", ScanOperator.Equal, status));
            }

            var scanSearch = _context.ScanAsync<TenantModel>(conditions);
            var allResults = await scanSearch.GetRemainingAsync();

            // Filtro de busca em memória (nome ou domínio)
            if (!string.IsNullOrWhiteSpace(search))
            {
                allResults = allResults.Where(t =>
                    t.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    t.Domain.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            var total = allResults.Count;

            // Paginação
            var tenants = allResults
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (tenants, total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todos os tenants");
            throw;
        }
    }

    public async Task CreateAsync(TenantModel tenant)
    {
        try
        {
            tenant.Id = Guid.NewGuid();
            tenant.PK = $"TENANT#{tenant.Id}";
            tenant.SK = "METADATA";
            tenant.EntityType = "TENANT";
            tenant.GSI1PK = $"DOMAIN#{tenant.Domain}";
            tenant.CreatedAt = DateTime.UtcNow;
            tenant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveAsync(tenant);

            _logger.LogInformation("Tenant {TenantName} criado com ID {TenantId}",
                tenant.Name, tenant.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar tenant: {TenantName}", tenant.Name);
            throw;
        }
    }

    public async Task UpdateAsync(TenantModel tenant)
    {
        try
        {
            tenant.UpdatedAt = DateTime.UtcNow;
            tenant.PK = $"TENANT#{tenant.Id}";
            tenant.SK = "METADATA";
            tenant.EntityType = "TENANT";
            tenant.GSI1PK = $"DOMAIN#{tenant.Domain}";

            await _context.SaveAsync(tenant);

            _logger.LogInformation("Tenant {TenantId} atualizado", tenant.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar tenant: {TenantId}", tenant.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            var tenant = await GetByIdAsync(id);

            if (tenant == null)
            {
                _logger.LogWarning("Tenant não encontrado para exclusão: {Id}", id);
                throw new KeyNotFoundException($"Tenant com ID {id} não encontrado");
            }

            tenant.Status = "deleted";
            tenant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveAsync(tenant);

            _logger.LogInformation("Tenant marcado como deletado: {Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar tenant: {Id}", id);
            throw;
        }
    }
}
