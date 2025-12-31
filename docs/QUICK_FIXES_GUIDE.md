# Guia de Correções Rápidas - Arda9 Tenant API

Este documento contém código pronto para implementar as correções mais críticas identificadas na análise da API.

---

## 🔴 Correção #1: CORS Seguro

### Problema
```csharp
// ❌ INSEGURO - Permite qualquer origem
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

### Solução
```csharp
// ✅ SEGURO - Apenas origens confiáveis
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? new[] { "https://app.arda9.com" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromHours(1));
    });
});
```

### Configuração (appsettings.json)
```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://app.arda9.com",
      "https://admin.arda9.com",
      "http://localhost:3000"
    ]
  }
}
```

**Arquivo:** `src/Arda9Tenant.Api/Program.cs` (linha 32-41)

---

## 🔴 Correção #2: Rate Limiting

### Solução Completa

**1. Adicionar pacote NuGet:**
```bash
cd src/Arda9Tenant.Api
dotnet add package Microsoft.AspNetCore.RateLimiting
```

**2. Configurar Rate Limiter no Program.cs:**
```csharp
using System.Threading.RateLimiting;

// Adicionar após builder.Services.AddCors(...)
builder.Services.AddRateLimiter(options =>
{
    // Rate limit geral da API
    options.AddFixedWindowLimiter("api", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });

    // Rate limit para criação de recursos (mais restritivo)
    options.AddFixedWindowLimiter("create", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    // Rate limit por tenant
    options.AddTokenBucketLimiter("per-tenant", opt =>
    {
        opt.TokenLimit = 1000;
        opt.ReplenishmentPeriod = TimeSpan.FromHours(1);
        opt.TokensPerPeriod = 1000;
        opt.AutoReplenishment = true;
    });

    // Resposta quando exceder limite
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Rate limit exceeded",
            retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                ? retryAfter.ToString()
                : "60 seconds"
        }, cancellationToken);
    };
});
```

**3. Habilitar middleware:**
```csharp
// Adicionar ANTES de app.UseAuthorization();
app.UseRateLimiter();
```

**4. Aplicar nos controllers:**
```csharp
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("api")] // Rate limit geral
public class TenantsController : ControllerBase
{
    // Endpoint de criação com rate limit mais restritivo
    [HttpPost]
    [EnableRateLimiting("create")]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantCommand command)
    {
        // ...
    }
}
```

**Arquivos afetados:**
- `src/Arda9Tenant.Api/Program.cs`
- `src/Arda9Tenant.Api/Controllers/TenantsController.cs`
- `src/Arda9Tenant.Api/Arda9Tenant.Api.csproj`

---

## 🔴 Correção #3: Validação de Audience

### Problema
```csharp
// ❌ Aceita tokens de qualquer aplicação do mesmo User Pool
ValidateAudience = false,
```

### Solução
```csharp
// ✅ Valida audience (Client ID do Cognito)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}",
            ValidateAudience = true, // ✅ Habilitado
            ValidAudience = cognitoConfig?.ClientId, // ✅ Client ID da aplicação
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero
        };
        
        // ✅ HTTPS obrigatório em produção
        var isDevelopment = builder.Environment.IsDevelopment();
        options.RequireHttpsMetadata = !isDevelopment;
    });
```

### Configuração (appsettings.json)
```json
{
  "AwsCognito": {
    "UserPoolId": "us-east-1_XXXXXXX",
    "Region": "us-east-1",
    "ClientId": "seu-client-id-aqui"
  }
}
```

### Modelo de Configuração
```csharp
// Arquivo: src/Arda9Tenant.Core/Configuration/AwsCognitoConfig.cs
namespace Core.Configuration;

public class AwsCognitoConfig
{
    public string UserPoolId { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty; // ✅ Adicionar
}
```

**Arquivo:** `src/Arda9Tenant.Api/Program.cs` (linhas 67-82)

---

## 🔴 Correção #4: Validação de Multi-Tenancy

### Problema
Usuários podem acessar dados de outros tenants se tiverem o ID.

### Solução: Middleware de Validação

**1. Criar classe de middleware:**
```csharp
// Arquivo: src/Arda9Tenant.Api/Middleware/TenantValidationMiddleware.cs
using System.Security.Claims;

namespace Arda9Tenant.Api.Middleware;

public class TenantValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantValidationMiddleware> _logger;

    public TenantValidationMiddleware(
        RequestDelegate next,
        ILogger<TenantValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Ignorar endpoints públicos e healthcheck
        var path = context.Request.Path.Value?.ToLower();
        if (path?.Contains("/health") == true || 
            path?.Contains("/swagger") == true)
        {
            await _next(context);
            return;
        }

        // Apenas para endpoints autenticados
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        // Extrair tenantId do token
        var tenantIdFromToken = context.User.FindFirst("custom:tenantId")?.Value;
        
        if (string.IsNullOrEmpty(tenantIdFromToken))
        {
            _logger.LogWarning("Token sem tenantId claim");
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new 
            { 
                error = "Invalid token: missing tenant information" 
            });
            return;
        }

        // Armazenar tenantId no HttpContext para uso nos handlers
        context.Items["TenantId"] = tenantIdFromToken;

        // Para endpoints que acessam recursos específicos de tenant
        // validar se o tenantId da rota corresponde ao do token
        if (context.Request.RouteValues.TryGetValue("id", out var routeId))
        {
            var endpoint = context.GetEndpoint();
            var controllerName = endpoint?.Metadata
                .GetMetadata<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>()
                ?.ControllerName;

            // Para TenantsController, validar se usuário pode acessar o tenant
            if (controllerName == "Tenants" && routeId != null)
            {
                // Aqui você pode adicionar lógica mais sofisticada:
                // - Super admin pode acessar qualquer tenant
                // - Usuários normais só podem acessar seu próprio tenant
                var role = context.User.FindFirst(ClaimTypes.Role)?.Value;
                var isSuperAdmin = role == "SuperAdmin";

                if (!isSuperAdmin && routeId.ToString() != tenantIdFromToken)
                {
                    _logger.LogWarning(
                        "Tentativa de acesso a tenant não autorizado. User Tenant: {UserTenant}, Requested: {RequestedTenant}",
                        tenantIdFromToken, routeId);
                    
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new 
                    { 
                        error = "Access denied: you don't have permission to access this tenant" 
                    });
                    return;
                }
            }
        }

        await _next(context);
    }
}

// Extension method para fácil registro
public static class TenantValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantValidation(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantValidationMiddleware>();
    }
}
```

**2. Registrar middleware no Program.cs:**
```csharp
// Adicionar APÓS app.UseAuthentication() e ANTES de app.UseAuthorization()
app.UseAuthentication();
app.UseTenantValidation(); // ✅ Novo middleware
app.UseAuthorization();
```

**3. Melhorar CurrentUserService:**
```csharp
// Arquivo: src/Arda9Tenant.Application/Services/CurrentUserService.cs
public Guid GetTenantId()
{
    var httpContext = _httpContextAccessor.HttpContext;
    if (httpContext?.User == null)
    {
        throw new UnauthorizedAccessException("User not authenticated");
    }

    // Tentar obter do HttpContext.Items (setado pelo middleware)
    if (httpContext.Items.TryGetValue("TenantId", out var tenantIdFromContext))
    {
        if (Guid.TryParse(tenantIdFromContext?.ToString(), out var tenantId))
        {
            return tenantId;
        }
    }

    // Fallback: extrair do claim
    var tenantIdClaim = httpContext.User.FindFirst("custom:tenantId")?.Value;
    
    if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantGuid))
    {
        throw new UnauthorizedAccessException("Invalid tenant information in token");
    }

    return tenantGuid;
}
```

**Arquivos novos:**
- `src/Arda9Tenant.Api/Middleware/TenantValidationMiddleware.cs`

**Arquivos modificados:**
- `src/Arda9Tenant.Api/Program.cs`
- `src/Arda9Tenant.Application/Services/CurrentUserService.cs`

---

## 🟡 Correção #5: Nomenclatura Consistente

### Problema
- AssemblyName: `Arda9Template.Api`
- Namespace: `Arda9Tenant.Api`
- Handler: `Arda9Tenency.Api`

### Solução

**1. Corrigir AssemblyName:**
```xml
<!-- Arquivo: src/Arda9Tenant.Api/Arda9Tenant.Api.csproj -->
<PropertyGroup>
  <TargetFramework>net8.0</TargetFramework>
  <AssemblyName>Arda9Tenant.Api</AssemblyName> <!-- ✅ Corrigido -->
  <AWSProjectType>Lambda</AWSProjectType>
  <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
  <PublishReadyToRun>true</PublishReadyToRun>
</PropertyGroup>
```

**2. Corrigir Handler no template.yaml:**
```yaml
# Arquivo: template.yaml
Resources:
  NetCodeWebAPIServerless:
    Type: AWS::Serverless::Function
    Properties:
      Description: Arda9 Tenant API - Multi-tenant management
      CodeUri: ./src/Arda9Tenant.Api/
      Handler: Arda9Tenant.Api  # ✅ Corrigido
      Runtime: dotnet8
      MemorySize: 1024
```

**3. Corrigir namespace no Controller (se necessário):**
```csharp
// Arquivo: src/Arda9Tenant.Api/Controllers/TenantsController.cs
// Mudar de:
namespace Arda9Tenency.Api.Controllers; // ❌

// Para:
namespace Arda9Tenant.Api.Controllers; // ✅
```

**Arquivos afetados:**
- `src/Arda9Tenant.Api/Arda9Tenant.Api.csproj`
- `template.yaml`
- `src/Arda9Tenant.Api/Controllers/TenantsController.cs`

---

## 🟡 Correção #6: TenantMaster Opcional

### Problema
TenantMasterId é obrigatório, mas como criar o primeiro tenant (root)?

### Solução

**1. Atualizar Validator:**
```csharp
// Arquivo: src/Arda9Tenant.Application/.../CreateTenantCommandValidator.cs
public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(200).WithMessage("Nome não pode ter mais de 200 caracteres");

        RuleFor(x => x.Domain)
            .NotEmpty().WithMessage("Domínio é obrigatório")
            .MaximumLength(100).WithMessage("Domínio não pode ter mais de 100 caracteres")
            .Matches(@"^[a-z0-9\-\.]+$").WithMessage("Domínio contém caracteres inválidos");

        RuleFor(x => x.Plan)
            .NotEmpty().WithMessage("Plano é obrigatório")
            .Must(plan => new[] { "basic", "pro", "enterprise" }.Contains(plan))
            .WithMessage("Plano deve ser: basic, pro ou enterprise");

        // ✅ TenantMasterId agora é opcional
        // RuleFor(x => x.TenantMasterId)
        //     .NotEmpty().WithMessage("TenantMasterId é obrigatório");

        RuleFor(x => x.PrimaryColor)
            .Matches(@"^#[0-9A-Fa-f]{6}$")
            .When(x => !string.IsNullOrEmpty(x.PrimaryColor))
            .WithMessage("Cor primária deve estar no formato hexadecimal (#RRGGBB)");

        RuleFor(x => x.SecondaryColor)
            .Matches(@"^#[0-9A-Fa-f]{6}$")
            .When(x => !string.IsNullOrEmpty(x.SecondaryColor))
            .WithMessage("Cor secundária deve estar no formato hexadecimal (#RRGGBB)");
    }
}
```

**2. Atualizar Handler:**
```csharp
// Arquivo: src/Arda9Tenant.Application/.../CreateTenantCommandHandler.cs
public async Task<Result<CreateTenantResponse>> Handle(
    CreateTenantCommand request, 
    CancellationToken cancellationToken)
{
    try
    {
        // ... código de autenticação ...

        // ✅ Verificar TenantMaster apenas se fornecido
        if (request.TenantMasterId.HasValue && request.TenantMasterId.Value != Guid.Empty)
        {
            var tenantMaster = await _tenantRepository.GetByIdAsync(request.TenantMasterId.Value);
            if (tenantMaster == null)
            {
                _logger.LogWarning("TenantMaster {TenantMasterId} not found", request.TenantMasterId);
                return Result<CreateTenantResponse>.Error("TenantMaster not found");
            }
        }

        // Validar se o domínio já existe
        if (await _tenantRepository.DomainExistsAsync(request.Domain))
        {
            _logger.LogWarning("Domain {Domain} already exists", request.Domain);
            return Result<CreateTenantResponse>.Error("Domain already exists");
        }

        var tenant = new TenantModel
        {
            Name = request.Name,
            Domain = request.Domain,
            TenantMaster = request.TenantMasterId ?? Guid.Empty, // ✅ Usa Empty se não fornecido
            CreatedBy = userGuid,
            PrimaryColor = request.PrimaryColor ?? "#0066cc",
            SecondaryColor = request.SecondaryColor ?? "#4d94ff",
            Plan = request.Plan,
            Status = "active"
        };

        await _tenantRepository.CreateAsync(tenant);

        _logger.LogInformation(
            "Tenant created successfully: {TenantId} - {TenantName} - TenantMaster: {TenantMaster}",
            tenant.Id, tenant.Name, 
            tenant.TenantMaster == Guid.Empty ? "ROOT" : tenant.TenantMaster.ToString());

        // ... resto do código ...
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating tenant: {TenantName}", request.Name);
        return Result<CreateTenantResponse>.Error("Error creating tenant");
    }
}
```

**Arquivos afetados:**
- `src/Arda9Tenant.Application/.../CreateTenantCommandValidator.cs`
- `src/Arda9Tenant.Application/.../CreateTenantCommandHandler.cs`

---

## 🟢 Correção #7: Healthcheck Endpoint

### Solução Simples

**1. Adicionar pacote (se necessário):**
```bash
dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks
```

**2. Configurar healthcheck:**
```csharp
// Program.cs - após builder.Services.AddAuthorization()
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
    .AddCheck("dynamodb", () =>
    {
        // Verificação simples - pode ser melhorada com ping real ao DynamoDB
        return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy();
    });
```

**3. Mapear endpoint:**
```csharp
// Antes de app.Run()
app.MapHealthChecks("/health");
```

**4. Excluir de autenticação:**
```csharp
// No TenantValidationMiddleware ou diretamente
app.MapHealthChecks("/health").AllowAnonymous();
```

**Arquivo:** `src/Arda9Tenant.Api/Program.cs`

---

## 📋 Checklist de Implementação

- [ ] Corrigir CORS (Correção #1)
- [ ] Implementar Rate Limiting (Correção #2)
- [ ] Habilitar Audience Validation (Correção #3)
- [ ] Adicionar Middleware de Tenant Validation (Correção #4)
- [ ] Corrigir nomenclatura (Correção #5)
- [ ] Tornar TenantMaster opcional (Correção #6)
- [ ] Adicionar Healthcheck (Correção #7)
- [ ] Testar localmente com `sam local start-api`
- [ ] Build e verificar warnings: `dotnet build`
- [ ] Deploy em ambiente de staging
- [ ] Testes de segurança
- [ ] Code review
- [ ] Deploy em produção

---

## 🧪 Como Testar

### 1. Teste Local
```bash
# Build
dotnet build

# SAM Local
sam build
sam local start-api

# Teste healthcheck
curl http://localhost:3000/health

# Teste CORS (deve rejeitar origem não autorizada)
curl -X GET http://localhost:3000/api/tenants \
  -H "Origin: https://malicious-site.com" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -v

# Teste Rate Limiting (fazer 101 requisições em 1 minuto)
for i in {1..101}; do
  curl http://localhost:3000/health
done
```

### 2. Teste de Segurança
```bash
# Teste com token de outro tenant (deve retornar 403)
curl -X GET http://localhost:3000/api/tenants/TENANT_ID_A \
  -H "Authorization: Bearer TOKEN_DO_TENANT_B" \
  -v
```

---

## 📚 Referências

- [ASP.NET Core Rate Limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit)
- [CORS in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/cors)
- [JWT Bearer Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/)
- [AWS SAM Documentation](https://docs.aws.amazon.com/serverless-application-model/)

---

**Última atualização:** 31/12/2024
