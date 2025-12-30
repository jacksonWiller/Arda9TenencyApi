# Refatoração: Upload de Logo do Tenant

## Mudanças Realizadas

A funcionalidade de upload de logo foi refatorada para **apenas atualizar a URL do logo** ao invés de fazer upload de arquivo físico. A URL será fornecida pela API de arquivos separada.

### Arquivos Modificados

#### 1. `UploadLogoCommand.cs`
**Antes:**
```csharp
public class UploadLogoCommand : IRequest<Result<UploadLogoResponse>>
{
    public Guid TenantId { get; set; }
    public IFormFile? File { get; set; }
}
```

**Depois:**
```csharp
public class UploadLogoCommand : IRequest<Result<UploadLogoResponse>>
{
    public Guid TenantId { get; set; }
    public string LogoUrl { get; set; } = string.Empty;
}
```

#### 2. `UploadLogoCommandHandler.cs`
**Mudanças:**
- ? Removida dependência de `IS3Service`
- ? Removida lógica de upload de arquivo para S3
- ? Removida validação de tipo de arquivo (extensões)
- ? Removida validação de tamanho de arquivo
- ? Adicionada validação de URL válida (HTTP/HTTPS)
- ? Simplificada para apenas atualizar a propriedade `Logo` do tenant

**Fluxo Atual:**
1. Valida se a URL foi fornecida
2. Valida se é uma URL válida (HTTP ou HTTPS)
3. Busca o tenant no repositório
4. Atualiza a propriedade `Logo` com a URL fornecida
5. Salva as alterações

#### 3. `TenantsController.cs`
**Antes:**
```csharp
[HttpPost("{id}/logo")]
[Consumes(MediaTypeNames.Multipart.FormData)]
public async Task<IActionResult> UploadLogo(Guid id, IFormFile file)
{
    var command = new UploadLogoCommand
    {
        TenantId = id,
        File = file
    };
    // ...
}
```

**Depois:**
```csharp
[HttpPatch("{id}/logo")]
[Consumes(MediaTypeNames.Application.Json)]
public async Task<IActionResult> UploadLogo(Guid id, [FromBody] UploadLogoCommand command)
{
    command.TenantId = id;
    var result = await _mediator.Send(command);
    return result.ToActionResult();
}
```

**Mudanças no Endpoint:**
- ? Método HTTP: `POST` ? `PATCH` (mais semântico para atualização)
- ? Content-Type: `multipart/form-data` ? `application/json`
- ? Parâmetro: `IFormFile` ? `UploadLogoCommand` (JSON body)

### Arquivos Criados

#### 4. `UploadLogoCommandValidator.cs` (NOVO)
Validator usando FluentValidation para garantir:
- ? TenantId não vazio
- ? LogoUrl não vazia
- ? LogoUrl é uma URL válida (HTTP ou HTTPS)
- ? LogoUrl não excede 2048 caracteres

## Como Usar

### Requisição Anterior (Upload de Arquivo)
```http
POST /api/tenants/{id}/logo
Content-Type: multipart/form-data

[arquivo binário]
```

### Nova Requisição (Atualizar URL)
```http
PATCH /api/tenants/{id}/logo
Content-Type: application/json
Authorization: Bearer {token}

{
  "logoUrl": "https://minha-api-de-arquivos.com/logos/logo-empresa.png"
}
```

### Exemplo de Resposta
```json
{
  "tenantId": "123e4567-e89b-12d3-a456-426614174000",
  "logoUrl": "https://minha-api-de-arquivos.com/logos/logo-empresa.png",
  "updatedAt": "2024-01-15T10:30:00Z"
}
```

## Validações Implementadas

1. **TenantId** - Obrigatório e não vazio
2. **LogoUrl** - Obrigatória, não vazia, deve ser URL válida (HTTP/HTTPS), máximo 2048 caracteres
3. **Tenant** - Deve existir no banco de dados

## Erros Possíveis

| Código | Situação | Mensagem |
|--------|----------|----------|
| 400 | URL não fornecida | "URL do logo não fornecida" |
| 400 | URL inválida | "URL do logo inválida" |
| 404 | Tenant não encontrado | "Tenant não encontrado" |
| 500 | Erro ao salvar | "Erro ao atualizar logo do tenant" |

## Benefícios da Refatoração

1. ? **Separação de Responsabilidades**: O serviço de tenants não precisa mais lidar com upload de arquivos
2. ? **Desacoplamento**: Não depende mais do S3Service
3. ? **Simplicidade**: Código mais simples e fácil de manter
4. ? **Flexibilidade**: Pode usar qualquer serviço de arquivos (S3, Azure Blob, CDN, etc.)
5. ? **Performance**: Não precisa processar upload de arquivo
6. ? **Reutilização**: Aproveita a infraestrutura existente da API de arquivos

## Fluxo Completo Sugerido

1. **Cliente**: Faz upload da imagem para a API de Arquivos
   ```http
   POST /api/files/{tenantId}
   Content-Type: multipart/form-data
   
   File: [logo.png]
   ```

2. **API de Arquivos**: Retorna a URL do arquivo
   ```json
   {
     "fileId": "...",
     "publicUrl": "https://cdn.example.com/logos/logo.png"
   }
   ```

3. **Cliente**: Atualiza o logo do tenant com a URL
   ```http
   PATCH /api/tenants/{id}/logo
   Content-Type: application/json
   
   {
     "logoUrl": "https://cdn.example.com/logos/logo.png"
   }
   ```

## Status da Build

? **Build Successful** - Todas as alterações foram compiladas com sucesso.

## Próximos Passos (Opcional)

- Considerar adicionar validação de tipo MIME na URL (verificar se é imagem)
- Adicionar webhook/callback para verificar se a URL está acessível
- Implementar cache de logos para melhor performance
- Adicionar suporte a múltiplas versões de logo (small, medium, large)
