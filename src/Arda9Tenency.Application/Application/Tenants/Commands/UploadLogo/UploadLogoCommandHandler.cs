//using Arda9Template.Application.Services;
//using Arda9Template.Domain.Repositories;
//using Core.Results;
//using MediatR;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Logging;

//namespace Arda9Template.Api.Application.Tenants.Commands.UploadLogo;

//public class UploadLogoCommandHandler : IRequestHandler<UploadLogoCommand, Result<UploadLogoResponse>>
//{
//    private readonly ITenantRepository _tenantRepository;
//    private readonly IS3Service _s3Service;
//    private readonly ILogger<UploadLogoCommandHandler> _logger;

//    public UploadLogoCommandHandler(
//        ITenantRepository tenantRepository,
//        IS3Service s3Service,
//        ILogger<UploadLogoCommandHandler> logger)
//    {
//        _tenantRepository = tenantRepository;
//        _s3Service = s3Service;
//        _logger = logger;
//    }

//    public async Task<Result<UploadLogoResponse>> Handle(UploadLogoCommand request, CancellationToken cancellationToken)
//    {
//        try
//        {
//            if (request.File == null || request.File.Length == 0)
//            {
//                return Result<UploadLogoResponse>.Failure("Arquivo não fornecido");
//            }

//            var tenant = await _tenantRepository.GetByIdAsync(request.TenantId);

//            if (tenant == null)
//            {
//                return Result<UploadLogoResponse>.Failure("Tenant não encontrado");
//            }

//            // Validar tipo de arquivo (apenas imagens)
//            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".svg" };
//            var fileExtension = Path.GetExtension(request.File.FileName).ToLower();

//            if (!allowedExtensions.Contains(fileExtension))
//            {
//                return Result<UploadLogoResponse>.Failure("Tipo de arquivo não permitido. Use: JPG, PNG, GIF ou SVG");
//            }

//            // Validar tamanho (max 5MB)
//            if (request.File.Length > 5 * 1024 * 1024)
//            {
//                return Result<UploadLogoResponse>.Failure("Arquivo muito grande. Tamanho máximo: 5MB");
//            }

//            // Upload para S3 (bucket de logos)
//            var bucketName = "arda9-tenants-logos"; // Bucket específico para logos
//            var key = $"tenants/{request.TenantId}/logo{fileExtension}";

//            using var stream = request.File.OpenReadStream();
//            var uploadResult = await _s3Service.UploadFileAsync(bucketName, key, stream, request.File.ContentType);

//            if (!uploadResult.IsSuccess)
//            {
//                return Result<UploadLogoResponse>.Failure("Erro ao fazer upload do logo");
//            }

//            // Atualizar tenant com a URL do logo
//            tenant.Logo = uploadResult.Value.Url;
//            await _tenantRepository.UpdateAsync(tenant);

//            _logger.LogInformation("Logo atualizado para tenant: {TenantId}", request.TenantId);

//            var response = new UploadLogoResponse
//            {
//                TenantId = tenant.Id,
//                LogoUrl = tenant.Logo,
//                UpdatedAt = tenant.UpdatedAt
//            };

//            return Result<UploadLogoResponse>.Success(response);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Erro ao fazer upload do logo para tenant: {TenantId}", request.TenantId);
//            return Result<UploadLogoResponse>.Failure("Erro ao fazer upload do logo");
//        }
//    }
//}
