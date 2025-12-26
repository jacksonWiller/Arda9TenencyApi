//using Arda9Template.Domain.Repositories;
//using Core.Results;
//using MediatR;
//using Microsoft.Extensions.Logging;

//namespace Arda9Template.Api.Application.Tenants.Commands.UpdateTenant;

//public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, Result<UpdateTenantResponse>>
//{
//    private readonly ITenantRepository _tenantRepository;
//    private readonly ILogger<UpdateTenantCommandHandler> _logger;

//    public UpdateTenantCommandHandler(
//        ITenantRepository tenantRepository,
//        ILogger<UpdateTenantCommandHandler> logger)
//    {
//        _tenantRepository = tenantRepository;
//        _logger = logger;
//    }

//    public async Task<Result<UpdateTenantResponse>> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
//    {
//        try
//        {
//            var tenant = await _tenantRepository.GetByIdAsync(request.Id);

//            if (tenant == null)
//            {
//                return Result<UpdateTenantResponse>.Failure("Tenant não encontrado");
//            }

//            // Atualizar apenas os campos fornecidos
//            if (!string.IsNullOrWhiteSpace(request.Name))
//                tenant.Name = request.Name;

//            if (!string.IsNullOrWhiteSpace(request.PrimaryColor))
//                tenant.PrimaryColor = request.PrimaryColor;

//            if (!string.IsNullOrWhiteSpace(request.SecondaryColor))
//                tenant.SecondaryColor = request.SecondaryColor;

//            if (!string.IsNullOrWhiteSpace(request.Status))
//                tenant.Status = request.Status;

//            if (!string.IsNullOrWhiteSpace(request.Plan))
//                tenant.Plan = request.Plan;

//            await _tenantRepository.UpdateAsync(tenant);

//            _logger.LogInformation("Tenant atualizado: {TenantId}", tenant.Id);

//            var response = new UpdateTenantResponse
//            {
//                Id = tenant.Id,
//                Name = tenant.Name,
//                Domain = tenant.Domain,
//                PrimaryColor = tenant.PrimaryColor,
//                SecondaryColor = tenant.SecondaryColor,
//                Status = tenant.Status,
//                Plan = tenant.Plan,
//                UpdatedAt = tenant.UpdatedAt
//            };

//            return Result<UpdateTenantResponse>.Success(response);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Erro ao atualizar tenant: {TenantId}", request.Id);
//            return Result<UpdateTenantResponse>.Failure("Erro ao atualizar tenant");
//        }
//    }
//}
