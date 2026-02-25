# Arda9 Tenant API - Guia de Execução Local

## ?? Como Rodar Localmente

Este projeto é uma **AWS Lambda Function** usando **.NET 8** com arquitetura em camadas. Existem 3 formas de rodar localmente:

---

## ?? Pré-requisitos

### **Ferramentas Necessárias**

1. **.NET 8 SDK**
   ```bash
   # Verificar instalação
   dotnet --version
   # Deve retornar 8.0.x
   ```

2. **AWS CLI** (para credenciais)
   ```bash
   # Verificar instalação
   aws --version
   
   # Configurar credenciais
   aws configure
   ```

3. **SAM CLI** (opcional, para simular Lambda)
   ```bash
   # Windows (Chocolatey)
   choco install aws-sam-cli
   
   # Ou baixe direto
   # https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html
   
   # Verificar instalação
   sam --version
   ```

4. **Docker** (opcional, para DynamoDB Local)
   ```bash
   # Verificar instalação
   docker --version
   ```

---

## ?? Método 1: Visual Studio / dotnet run (Recomendado)

### **Vantagens**
- ? Mais rápido para desenvolvimento
- ? Hot reload habilitado
- ? Debug completo com breakpoints
- ? Swagger UI disponível

### **Passo a Passo**

#### **1. Configurar Credenciais AWS**
```bash
aws configure
# Informe: Access Key, Secret Key, Region (us-east-1)
```

#### **2. Restaurar Dependências**
```bash
# Na raiz do projeto
dotnet restore
```

#### **3. Build do Projeto**
```bash
dotnet build
```

#### **4. Rodar o Projeto**

**Opção A: Visual Studio**
1. Abra o projeto no Visual Studio
2. Selecione o perfil **"ServerlessAPI"** no dropdown
3. Pressione **F5** ou clique em "Run"

**Opção B: Linha de Comando**
```bash
cd src/Arda9Tenant.Api
dotnet run
```

#### **5. Acessar a API**

- **Swagger UI**: https://localhost:5001
- **HTTPS**: https://localhost:5001
- **HTTP**: http://localhost:5000

### **Variáveis de Ambiente (Automáticas)**

O arquivo `launchSettings.json` já configura:
```json
{
  "ASPNETCORE_ENVIRONMENT": "Development",
  "AWS_REGION": "us-east-1"
}
```

### **?? Erro de Porta em Uso?**

Se você receber erro de porta, veja a documentação completa em `docs/SOLUCAO_ERRO_PORTA.md`

**Solução Rápida:**
```powershell
# Verificar processo na porta
netstat -ano | findstr :5001

# Matar processo (substitua PID)
taskkill /PID <PID> /F
```

---

## ?? Método 2: SAM CLI Local (Simula Lambda)

### **Vantagens**
- ? Ambiente idêntico ao Lambda
- ? Testa configurações do template.yaml
- ? Simula API Gateway

### **Passo a Passo**

#### **1. Build com SAM**
```bash
# Na raiz do projeto (onde está o template.yaml)
sam build
```

O SAM CLI irá:
- Compilar o projeto `Arda9Tenant.Api`
- Resolver todas as dependências das camadas (Application, Core, Domain, Infra)
- Criar um pacote Lambda completo em `.aws-sam/build/`

#### **2. Iniciar API Local**
```bash
sam local start-api
```

#### **3. Acessar a API**
- **Base URL**: http://localhost:3000
- **Swagger**: http://localhost:3000/swagger (se habilitado)

#### **4. (Opcional) Invocar Função Diretamente**
```bash
# Criar arquivo de evento de teste
sam local invoke NetCodeWebAPIServerless --event events/test-event.json
```

### **Exemplo de Requisição**
```bash
# Listar tenants
curl http://localhost:3000/api/tenants \
  -H "Authorization: Bearer {seu-token-jwt}"
```

---

## ?? Método 3: AWS Lambda Test Tool

### **Vantagens**
- ? Interface gráfica para testar Lambda
- ? Mock de eventos AWS
- ? Debugging visual

### **Passo a Passo**

#### **1. Instalar Lambda Test Tool**
```bash
dotnet tool install -g Amazon.Lambda.TestTool-8.0
```

#### **2. Rodar o Test Tool**

**Opção A: Visual Studio**
1. Selecione o perfil **"Mock Lambda Test Tool"**
2. Execute (F5)

**Opção B: Linha de Comando**
```bash
cd src/Arda9Tenant.Api
dotnet lambda-test-tool-8.0 --port 5050
```

#### **3. Acessar a Interface**
- **URL**: http://localhost:5050
- **Interface**: Mock de eventos Lambda

---

## ??? DynamoDB Local (Opcional)

### **Por que usar?**
- ? Desenvolvimento offline
- ? Testes sem custo AWS
- ? Dados isolados

### **Configuração**

#### **1. Iniciar DynamoDB Local (Docker)**
```bash
docker run -p 8000:8000 amazon/dynamodb-local
```

#### **2. Criar Tabela**
```bash
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
        '[
          {
            "IndexName": "GSI1-Index",
            "KeySchema": [
              {"AttributeName": "GSI1PK", "KeyType": "HASH"},
              {"AttributeName": "GSI1SK", "KeyType": "RANGE"}
            ],
            "Projection": {"ProjectionType": "ALL"}
          },
          {
            "IndexName": "GSI2-Index",
            "KeySchema": [
              {"AttributeName": "GSI2PK", "KeyType": "HASH"},
              {"AttributeName": "GSI2SK", "KeyType": "RANGE"}
            ],
            "Projection": {"ProjectionType": "ALL"}
          },
          {
            "IndexName": "GSI3-Index",
            "KeySchema": [
              {"AttributeName": "GSI3PK", "KeyType": "HASH"}
            ],
            "Projection": {"ProjectionType": "ALL"}
          }
        ]' \
    --endpoint-url http://localhost:8000
```

#### **3. Configurar Endpoint Local no Código**

Adicione no `appsettings.Development.json`:
```json
{
  "AWS": {
    "DynamoDBLocalEndpoint": "http://localhost:8000"
  }
}
```

Modifique `Program.cs` para usar endpoint local:
```csharp
// Configuração da região AWS
string awsRegion = Environment.GetEnvironmentVariable("AWS_REGION") ?? RegionEndpoint.USEast1.SystemName;
var regionEndpoint = RegionEndpoint.GetBySystemName(awsRegion);

// Configuração DynamoDB (usar local se disponível)
var dynamoConfig = new AmazonDynamoDBConfig { RegionEndpoint = regionEndpoint };
if (builder.Environment.IsDevelopment())
{
    var localEndpoint = builder.Configuration["AWS:DynamoDBLocalEndpoint"];
    if (!string.IsNullOrEmpty(localEndpoint))
    {
        dynamoConfig.ServiceURL = localEndpoint;
    }
}

builder.Services.AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient(dynamoConfig));
```

---

## ?? Autenticação JWT (Cognito)

### **Configuração no appsettings.json**

```json
{
  "AwsCognito": {
    "UserPoolId": "us-east-1_tg7PHhZle",
    "ClientId": "6gdd4f0r9k274c4ilann8k5jm7",
    "ClientSecret": "9a131vl8cmfo5ovins06c3245fnjmcej81m72dpre5cvlva6477",
    "Region": "us-east-1"
  }
}
```

### **Obter Token JWT**

```bash
# Autenticar usuário no Cognito
aws cognito-idp initiate-auth \
    --auth-flow USER_PASSWORD_AUTH \
    --client-id 6gdd4f0r9k274c4ilann8k5jm7 \
    --auth-parameters USERNAME=usuario@email.com,PASSWORD=SuaSenha123! \
    --region us-east-1
```

### **Testar Endpoint Protegido**

```bash
curl https://localhost:5001/api/tenants \
  -H "Authorization: Bearer eyJraWQiOiJ...seu-token-aqui"
```

---

## ?? Testar a API

### **Swagger UI**
1. Acesse: https://localhost:5001
2. Clique em **"Authorize"**
3. Informe o token JWT: `Bearer {seu-token}`
4. Teste os endpoints

### **cURL**

```bash
# Criar Tenant
curl -X POST https://localhost:5001/api/tenants \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Tenant Teste",
    "domain": "teste.com",
    "isActive": true
  }'

# Listar Tenants
curl https://localhost:5001/api/tenants \
  -H "Authorization: Bearer {token}"

# Obter Tenant por ID
curl https://localhost:5001/api/tenants/{tenant-id} \
  -H "Authorization: Bearer {token}"
```

### **Postman**
1. Importe a collection (se disponível)
2. Configure o token JWT no header `Authorization`
3. Teste os endpoints

---

## ?? Troubleshooting

### **Problema 1: Erro "Unable to resolve service for type 'IAmazonDynamoDB'"**

**Causa**: Credenciais AWS não configuradas

**Solução**:
```bash
aws configure
# Informe Access Key, Secret Key, Region
```

---

### **Problema 2: Porta em uso (50364 ou 50365)**

**Solução**: Altere as portas no `launchSettings.json`:
```json
{
  "applicationUrl": "https://localhost:5001;http://localhost:5000"
}
```

---

### **Problema 3: Erro "Handler not found" no SAM CLI**

**Causa**: AssemblyName não corresponde ao Handler no template.yaml

**Verificação**:
- **template.yaml**: `Handler: Arda9Tenant.Api`
- **.csproj**: `<AssemblyName>Arda9Tenant.Api</AssemblyName>`

---

### **Problema 4: Erro de autenticação JWT**

**Causa**: Token inválido ou expirado

**Solução**: Obtenha novo token do Cognito:
```bash
aws cognito-idp initiate-auth \
    --auth-flow USER_PASSWORD_AUTH \
    --client-id 6gdd4f0r9k274c4ilann8k5jm7 \
    --auth-parameters USERNAME=usuario@email.com,PASSWORD=Senha123! \
    --region us-east-1
```

---

## ?? Comandos Úteis

```bash
# Build do projeto
dotnet build

# Restaurar dependências
dotnet restore

# Limpar build
dotnet clean

# Rodar testes
dotnet test

# Publicar (criar pacote)
dotnet publish -c Release

# SAM Build
sam build

# SAM Local API
sam local start-api

# SAM Validate
sam validate

# SAM Deploy
sam deploy --guided
```

---

## ?? Deploy na AWS

### **Primeira vez (Guided)**
```bash
sam deploy --guided
```

Responda as perguntas:
- Stack Name: `arda9-tenant-api`
- AWS Region: `us-east-1`
- Confirm changes: `Y`
- Allow SAM CLI IAM role creation: `Y`
- Save arguments: `Y`

### **Deploys Subsequentes**
```bash
sam deploy
```

### **Validar Stack**
```bash
aws cloudformation describe-stacks \
    --stack-name arda9-tenant-api \
    --region us-east-1
```

---

## ?? Recursos Adicionais

- **Documentação SAM**: https://docs.aws.amazon.com/serverless-application-model/
- **Documentação .NET Lambda**: https://docs.aws.amazon.com/lambda/latest/dg/csharp-handler.html
- **DynamoDB Local**: https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.html
- **Cognito Authentication**: https://docs.aws.amazon.com/cognito/

---

## ?? Estrutura do Projeto

```
arda9-tenant-api/
??? src/
?   ??? Arda9Tenant.Api/          # Entry Point - Lambda Handler
?   ??? Arda9Tenant.Application/  # Casos de Uso (CQRS + MediatR)
?   ??? Arda9Tenant.Core/          # Abstrações e Modelos Base
?   ??? Arda9Tenant.Domain/        # Entidades e Interfaces
?   ??? Arda9Tenant.Infra/         # Implementação de Repositórios
??? docs/
?   ??? ARQUITETURA_CAMADAS_SAM_TENANT.md
??? template.yaml                  # SAM Template
??? README_LOCAL.md                # Este arquivo
```

---

## ? Checklist de Validação

Antes de fazer push para produção:

- [ ] `dotnet build` executa sem erros
- [ ] `sam build` executa sem erros
- [ ] `sam local start-api` funciona corretamente
- [ ] Swagger UI carrega em https://localhost:5001
- [ ] Autenticação JWT funciona
- [ ] DynamoDB responde corretamente
- [ ] Todos os testes passam (`dotnet test`)
- [ ] Deploy na AWS funciona (`sam deploy`)

---

**Última Atualização**: 2024  
**Versão**: 1.0  
**.NET**: 8.0  
**SAM CLI**: Recomendado 1.100+
