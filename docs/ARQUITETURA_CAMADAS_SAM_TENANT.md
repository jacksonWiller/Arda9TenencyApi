# Arquitetura em Camadas - Arda9 Tenant API

## ? Estrutura do Projeto

```
arda9-tenant-api/
??? src/
?   ??? Arda9Tenant.Api/          # Entry Point - Lambda Handler
?   ??? Arda9Tenant.Application/  # Casos de Uso (CQRS + MediatR)
?   ??? Arda9Tenant.Core/          # Abstrações e Modelos Base
?   ??? Arda9Tenant.Domain/        # Entidades e Interfaces
?   ??? Arda9Tenant.Infra/         # Implementação de Repositórios
??? template.yaml                  # SAM Template
??? docs/
```

---

## ?? Dependências por Camada

### **Arda9Tenant.Core** (Base)
**Responsabilidade**: Abstrações, interfaces base, modelos compartilhados

**Pacotes NuGet**:
- Ardalis.Result
- AWSSDK.DynamoDBv2 (atributos de mapeamento)
- FluentValidation
- MediatR
- Microsoft.AspNetCore.Http.Abstractions
- Microsoft.AspNetCore.Mvc.Core

**ProjectReferences**: Nenhum ?

---

### **Arda9Tenant.Domain** (Modelos)
**Responsabilidade**: Entidades, Value Objects, Interfaces de Repositórios

**Pacotes NuGet**:
- AWSSDK.DynamoDBv2

**ProjectReferences**:
- Arda9Tenant.Core

---

### **Arda9Tenant.Application** (Casos de Uso)
**Responsabilidade**: Commands, Queries, Handlers (CQRS), DTOs, Validators

**Pacotes NuGet**:
- Ardalis.Result
- AutoMapper
- AWSSDK.S3
- FluentValidation
- MediatR
- Microsoft.Extensions.Logging.Abstractions

**ProjectReferences**:
- Arda9Tenant.Core
- Arda9Tenant.Domain

---

### **Arda9Tenant.Infra** (Implementação)
**Responsabilidade**: Implementação de Repositórios, Acesso a Dados AWS

**Pacotes NuGet**:
- AWSSDK.DynamoDBv2
- AWSSDK.S3
- AWSSDK.S3Control
- AWSSDK.CognitoIdentityProvider
- AWSSDK.SecretsManager
- Microsoft.AspNetCore.Http.Abstractions

**ProjectReferences**:
- Arda9Tenant.Application
- Arda9Tenant.Core
- Arda9Tenant.Domain

---

### **Arda9Tenant.Api** (Entry Point)
**Responsabilidade**: Controllers, Program.cs, Configuração DI, Lambda Handler

**Pacotes NuGet**:
- Amazon.Lambda.AspNetCoreServer.Hosting
- Ardalis.Result
- AutoMapper
- AWSSDK.* (todos os SDKs necessários)
- FluentValidation
- MediatR
- Microsoft.AspNetCore.Authentication.JwtBearer
- Swashbuckle.AspNetCore

**ProjectReferences**:
- Arda9Tenant.Application
- Arda9Tenant.Core
- Arda9Tenant.Domain
- Arda9Tenant.Infra

**Configuração Crítica**:
```xml
<AssemblyName>Arda9Tenant.Api</AssemblyName>
<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
<PublishReadyToRun>true</PublishReadyToRun>
```

---

## ?? Template SAM (template.yaml)

### **Configuração Lambda**

```yaml
Resources:
  NetCodeWebAPIServerless:
    Type: AWS::Serverless::Function
    Properties:
      Description: A simple example includes a .NET Core WebAPI App with DynamoDB table.
      CodeUri: ./src/Arda9Tenant.Api/  # Aponta para o projeto API
      Handler: Arda9Tenant.Api          # Deve corresponder ao AssemblyName
      Runtime: dotnet8
      MemorySize: 1024
```

### **? Como o SAM Compila Arquitetura em Camadas**

1. **SAM CLI** executa `dotnet publish` no projeto `Arda9Tenant.Api`
2. O MSBuild resolve **todas as dependências** através dos `ProjectReference`
3. Todas as DLLs das camadas são copiadas para a pasta de output
4. `CopyLocalLockFileAssemblies=true` garante que todas as dependências sejam incluídas
5. O pacote Lambda contém **todos os assemblies necessários**

**Você NÃO precisa**:
- ? Adicionar referências explícitas às camadas no template.yaml
- ? Configurar múltiplos CodeUri
- ? Criar builds separados para cada camada

**O SAM faz tudo automaticamente!** ?

---

## ??? Como Rodar Localmente

### **Opção 1: Visual Studio / dotnet run**

```bash
# 1. Build do projeto
dotnet build

# 2. Rodar API local
cd src/Arda9Tenant.Api
dotnet run

# Acesse: https://localhost:50364
```

### **Opção 2: SAM CLI (Simula Lambda)**

```bash
# 1. Build com SAM (compila todas as camadas)
sam build

# 2. Rodar API local
sam local start-api

# Acesse: http://localhost:3000
```

### **Opção 3: Lambda Test Tool**

```bash
# Instalar ferramenta
dotnet tool install -g Amazon.Lambda.TestTool-8.0

# Rodar
cd src/Arda9Tenant.Api
dotnet lambda-test-tool-8.0 --port 5050

# Acesse: http://localhost:5050
```

---

## ?? Fluxo de Build e Deploy

```bash
# 1. Validar template SAM
sam validate

# 2. Build (compila todas as camadas)
sam build

# 3. Testar localmente
sam local start-api

# 4. Deploy na AWS
sam deploy --guided

# Ou deploy direto (após primeira configuração)
sam deploy
```

---

## ?? Problemas Comuns e Soluções

### **1. Handler não corresponde ao AssemblyName**

**Erro**:
```yaml
Handler: Arda9Tenant.Api  # No template.yaml
```

```xml
<AssemblyName>Arda9Template.Api</AssemblyName>  <!-- No .csproj -->
```

**Solução**: Garantir que o `AssemblyName` no `.csproj` corresponda ao `Handler` no template.

---

### **2. Internal Server Error no Lambda**

**Causas Comuns**:
- AssemblyName incorreto
- Falta de `CopyLocalLockFileAssemblies=true`
- Handler apontando para namespace errado
- Dependências não copiadas

**Solução**: Verificar logs no CloudWatch e garantir configuração correta do `.csproj`

---

### **3. DynamoDB Local não conecta**

```bash
# Instalar DynamoDB Local
docker run -p 8000:8000 amazon/dynamodb-local

# Criar tabela
aws dynamodb create-table \
    --table-name arda9-tenant-v2 \
    --attribute-definitions \
        AttributeName=PK,AttributeType=S \
        AttributeName=SK,AttributeType=S \
        AttributeName=GSI1PK,AttributeType=S \
        AttributeName=GSI1SK,AttributeType=S \
        AttributeName=GSI2PK,AttributeType=S \
        AttributeName=GSI2SK,AttributeType=S \
        AttributeName=GSI3PK,AttributeType=S \
    --key-schema \
        AttributeName=PK,KeyType=HASH \
        AttributeName=SK,KeyType=RANGE \
    --billing-mode PAY_PER_REQUEST \
    --global-secondary-indexes \
        IndexName=GSI1-Index,KeySchema=["{AttributeName=GSI1PK,KeyType=HASH}","{AttributeName=GSI1SK,KeyType=RANGE}"],Projection="{ProjectionType=ALL}" \
        IndexName=GSI2-Index,KeySchema=["{AttributeName=GSI2PK,KeyType=HASH}","{AttributeName=GSI2SK,KeyType=RANGE}"],Projection="{ProjectionType=ALL}" \
        IndexName=GSI3-Index,KeySchema=["{AttributeName=GSI3PK,KeyType=HASH}"],Projection="{ProjectionType=ALL}" \
    --endpoint-url http://localhost:8000
```

---

## ??? Princípios de Arquitetura

### **1. Separação de Responsabilidades**
- **Core**: Contratos e abstrações
- **Domain**: Regras de negócio puras
- **Application**: Orquestração de casos de uso
- **Infra**: Implementação técnica
- **Api**: Exposição HTTP e configuração

### **2. Dependência Unidirecional**
```
Api ? Application ? Domain ? Core
  ?       ?
Infra  ? Domain
```

### **3. Dependency Inversion Principle (DIP)**
- Interfaces definidas em **Domain**
- Implementações em **Infra**
- Inversão de controle em **Api** (Program.cs)

### **4. CQRS com MediatR**
- **Commands**: Alteram estado
- **Queries**: Apenas leitura
- **Handlers**: Executam lógica de negócio

---

## ?? Recursos AWS

### **DynamoDB Table: arda9-tenant-v2**

**Chaves Primárias**:
- **PK** (Partition Key): Hash
- **SK** (Sort Key): Range

**Global Secondary Indexes**:
1. **GSI1-Index**: Queries por Bucket
2. **GSI2-Index**: Queries por Folder
3. **GSI3-Index**: Queries por Company

**Billing Mode**: PAY_PER_REQUEST (On-Demand)

---

## ? Checklist de Validação

- [x] AssemblyName corresponde ao Handler
- [x] CopyLocalLockFileAssemblies está habilitado
- [x] ProjectReferences corretos em cada camada
- [x] Dependências mínimas por camada
- [x] Template.yaml aponta para o projeto correto (Api)
- [x] Todas as camadas compilam sem erros
- [x] SAM build funciona
- [x] SAM local start-api funciona
- [x] Deploy na AWS funciona

---

## ?? Referências

- [AWS SAM Documentation](https://docs.aws.amazon.com/serverless-application-model/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [MediatR](https://github.com/jbogard/MediatR)

---

**Última Atualização**: 2024
**Versão do .NET**: 8.0
**Versão do SAM CLI**: Recomendado 1.100+
