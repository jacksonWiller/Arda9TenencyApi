using System.Net.Mime;
using Arda9Tenant.Api.Application.Tenants.Commands.CreateTenant;
using Arda9Tenant.Api.Application.Tenants.Commands.DeleteTenant;
using Arda9Tenant.Api.Application.Tenants.Commands.UpdateTenant;
using Arda9Tenant.Api.Application.Tenants.Commands.UploadLogo;
using Arda9Tenant.Api.Application.Tenants.Queries.GetAllTenants;
using Arda9Tenant.Api.Application.Tenants.Queries.GetTenantById;
using Core.Api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arda9Tenency.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Authorize]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TenantsController> _logger;

    public TenantsController(IMediator mediator, ILogger<TenantsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Lista todos os tenants com suporte a paginação e filtros
    /// </summary>
    /// <param name="query">Parâmetros de busca e paginação</param>
    /// <returns>Lista paginada de tenants</returns>
    /// <response code="200">Tenants encontrados</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="500">Erro interno</response>
    [HttpGet]
    [ProducesResponseType(typeof(GetAllTenantsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllTenants([FromQuery] GetAllTenantsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtém detalhes de um tenant específico
    /// </summary>
    /// <param name="id">ID do tenant</param>
    /// <returns>Informações do tenant</returns>
    /// <response code="200">Tenant encontrado</response>
    /// <response code="404">Tenant não encontrado</response>
    /// <response code="500">Erro interno</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GetTenantByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTenantById(Guid id)
    {
        var query = new GetTenantByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    /// <summary>
    /// Cria um novo tenant
    /// </summary>
    /// <param name="command">Dados do tenant</param>
    /// <returns>Informações do tenant criado</returns>
    /// <response code="201">Tenant criado com sucesso</response>
    /// <response code="400">Parâmetros inválidos</response>
    /// <response code="500">Erro interno</response>
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(CreateTenantResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Atualiza dados de um tenant
    /// </summary>
    /// <param name="id">ID do tenant</param>
    /// <param name="command">Dados a serem atualizados</param>
    /// <returns>Informações do tenant atualizado</returns>
    /// <response code="200">Tenant atualizado com sucesso</response>
    /// <response code="400">Parâmetros inválidos</response>
    /// <response code="404">Tenant não encontrado</response>
    /// <response code="500">Erro interno</response>
    [HttpPatch("{id}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UpdateTenantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateTenant(Guid id, [FromBody] UpdateTenantCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Remove um tenant (soft delete)
    /// </summary>
    /// <param name="id">ID do tenant</param>
    /// <returns>Status da operação</returns>
    /// <response code="200">Tenant removido com sucesso</response>
    /// <response code="404">Tenant não encontrado</response>
    /// <response code="500">Erro interno</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteTenant(Guid id)
    {
        var command = new DeleteTenantCommand { Id = id };
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Atualiza o logo do tenant
    /// </summary>
    /// <param name="id">ID do tenant</param>
    /// <param name="command">URL do logo</param>
    /// <returns>URL do logo atualizado</returns>
    /// <response code="200">Logo atualizado com sucesso</response>
    /// <response code="400">URL inválida</response>
    /// <response code="404">Tenant não encontrado</response>
    /// <response code="500">Erro interno</response>
    [HttpPatch("{id}/logo")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UploadLogoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadLogo(Guid id, [FromBody] UploadLogoCommand command)
    {
        command.TenantId = id;
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}
