# Template SAM - Análise Detalhada

## ?? Visão Geral

O arquivo `template.yaml` define a infraestrutura da aplicação **Arda9 Tenant API** usando **AWS SAM (Serverless Application Model)**.

---

## ??? Estrutura Completa

```yaml
AWSTemplateFormatVersion: '2010-09-09'
Transform: AWS::Serverless-2016-10-31
Description: Sample SAM Template for Arda9Tenency API

Globals:
  Function:
    Timeout: 100

Resources:
  NetCodeWebAPIServerless:
    Type: AWS::Serverless::Function
    Properties:
      Description: A simple example includes a .NET Core WebAPI App with DynamoDB table.
      CodeUri: ./src/Arda9Tenant.Api/
      Handler: Arda9Tenant.Api
      Runtime: dotnet8
      MemorySize: 1024
      Environment:
        Variables:
          SAMPLE_TABLE: !Ref SampleTable
      Policies:
        - DynamoDBCrudPolicy:
            TableName: !Ref SampleTable
      Events:
        ProxyResource:
          Type: HttpApi
          Properties:
            PayloadFormatVersion: "2.0"
            Path: /{proxy+}
            Method: ANY
        RootResource:
          Type: HttpApi
          Properties:
            PayloadFormatVersion: "2.0"
            Path: /
            Method: ANY

  SampleTable:
    Type: AWS::DynamoDB::Table
    Properties:
      TableName: arda9-tenant-v2
      BillingMode: PAY_PER_REQUEST
      # ... (configuração completa abaixo)
```

---

## ?? Análise por Seção

### **1. Globals**

```yaml
Globals:
  Function:
    Timeout: 100
```

**Configuração**:
- **Timeout**: 100 segundos
- Aplicado a **todas as funções Lambda** no template

**Recomendações**:
- API típica: 30-60 segundos
- Processamento pesado: 60-300 segundos
- Máximo Lambda: 900 segundos (15 minutos)

---

### **2. Lambda Function**

```yaml
NetCodeWebAPIServerless:
  Type: AWS::Serverless::Function
  Properties:
    CodeUri: ./src/Arda9Tenant.Api/
    Handler: Arda9Tenant.Api
    Runtime: dotnet8
    MemorySize: 1024
```

#### **CodeUri**
- **Valor**: `./src/Arda9Tenant.Api/`
- **Função**: Aponta para o projeto .NET que contém o código da Lambda
- **SAM CLI**: Executa `dotnet publish` neste diretório
- **Importante**: Todas as camadas (Application, Core, Domain, Infra) são incluídas automaticamente via `ProjectReference`

#### **Handler**
- **Valor**: `Arda9Tenant.Api`
- **Formato**: `AssemblyName` (sem extensão `.dll`)
- **Validação**: Deve corresponder ao `<AssemblyName>` no `.csproj`

**Exemplo de .csproj**:
```xml
<PropertyGroup>
  <AssemblyName>Arda9Tenant.Api</AssemblyName>
</PropertyGroup>
```

#### **Runtime**
- **Valor**: `dotnet8`
- **Versões Suportadas**: `dotnet6`, `dotnet8`
- **Importante**: Deve corresponder ao `<TargetFramework>` no `.csproj`

#### **MemorySize**
- **Valor**: `1024 MB` (1 GB)
- **Faixa**: 128 MB - 10.240 MB (em incrementos de 1 MB)
- **Custo**: Maior memória = maior CPU + maior custo
- **Recomendação**:
  - API simples: 512-1024 MB
  - API com processamento: 1024-2048 MB
  - Processamento pesado: 2048+ MB

---

### **3. Environment Variables**

```yaml
Environment:
  Variables:
    SAMPLE_TABLE: !Ref SampleTable
```

**Configuração Atual**:
- `SAMPLE_TABLE`: Nome da tabela DynamoDB (referência dinâmica)

**Recomendações Adicionais**:
```yaml
Environment:
  Variables:
    SAMPLE_TABLE: !Ref SampleTable
    AWS_REGION: !Ref AWS::Region
    ASPNETCORE_ENVIRONMENT: Production
    LOG_LEVEL: Information
```

---

### **4. Policies (IAM Permissions)**

```yaml
Policies:
  - DynamoDBCrudPolicy:
      TableName: !Ref SampleTable
```

**Permissões Concedidas**:
- `dynamodb:GetItem`
- `dynamodb:PutItem`
- `dynamodb:UpdateItem`
- `dynamodb:DeleteItem`
- `dynamodb:Query`
- `dynamodb:Scan`

**Recomendações Adicionais** (se necessário):
```yaml
Policies:
  - DynamoDBCrudPolicy:
      TableName: !Ref SampleTable
  - S3CrudPolicy:
      BucketName: arda9-tenant-files
  - Statement:
    - Effect: Allow
      Action:
        - cognito-idp:AdminGetUser
        - cognito-idp:AdminUpdateUserAttributes
      Resource: !Sub arn:aws:cognito-idp:${AWS::Region}:${AWS::AccountId}:userpool/*
```

---

### **5. Events (API Gateway)**

#### **ProxyResource**
```yaml
ProxyResource:
  Type: HttpApi
  Properties:
    PayloadFormatVersion: "2.0"
    Path: /{proxy+}
    Method: ANY
```

**Configuração**:
- **Tipo**: HTTP API (mais barato e simples que REST API)
- **Path**: `/{proxy+}` (captura todas as rotas)
- **Method**: `ANY` (GET, POST, PUT, DELETE, etc.)
- **PayloadFormatVersion**: `2.0` (formato de evento mais limpo)

**Rotas Capturadas**:
- `/api/tenants`
- `/api/tenants/{id}`
- `/swagger`
- Qualquer outra rota definida nos controllers

#### **RootResource**
```yaml
RootResource:
  Type: HttpApi
  Properties:
    PayloadFormatVersion: "2.0"
    Path: /
    Method: ANY
```

**Configuração**:
- **Path**: `/` (rota raiz)
- **Uso**: Swagger UI, health check, página inicial

---

### **6. DynamoDB Table**

```yaml
SampleTable:
  Type: AWS::DynamoDB::Table
  Properties:
    TableName: arda9-tenant-v2
    BillingMode: PAY_PER_REQUEST
```

#### **TableName**
- **Valor**: `arda9-tenant-v2`
- **Importante**: Nome único na região AWS

#### **BillingMode**
- **Valor**: `PAY_PER_REQUEST` (On-Demand)
- **Alternativa**: `PROVISIONED` (capacidade reservada)
- **Recomendação**: `PAY_PER_REQUEST` para workloads imprevisíveis

---

### **7. Chaves e Índices**

#### **Primary Key**
```yaml
KeySchema:
  - AttributeName: PK
    KeyType: HASH    # Partition Key
  - AttributeName: SK
    KeyType: RANGE   # Sort Key
```

**Single Table Design**:
- **PK** (Partition Key): Agrupa itens relacionados
- **SK** (Sort Key): Ordena itens dentro da partição

**Exemplos de Padrões**:
```
PK                    SK                   Tipo
---------------------------------------------
TENANT#123           METADATA             Tenant
TENANT#123           BUCKET#456           Bucket
TENANT#123           FOLDER#789           Folder
BUCKET#456           FILE#abc             File
```

#### **Global Secondary Indexes (GSI)**

**GSI1: Queries por Bucket**
```yaml
- IndexName: GSI1-Index
  KeySchema:
    - AttributeName: GSI1PK
      KeyType: HASH
    - AttributeName: GSI1SK
      KeyType: RANGE
  Projection:
    ProjectionType: ALL
```

**Uso**: Listar arquivos/folders por bucket
```
GSI1PK           GSI1SK            Descrição
-----------------------------------------------
BUCKET#456       FILE#abc          Arquivo no bucket
BUCKET#456       FOLDER#789        Folder no bucket
```

**GSI2: Queries por Folder**
```yaml
- IndexName: GSI2-Index
  KeySchema:
    - AttributeName: GSI2PK
      KeyType: HASH
    - AttributeName: GSI2SK
      KeyType: RANGE
```

**Uso**: Listar arquivos por folder
```
GSI2PK           GSI2SK            Descrição
-----------------------------------------------
FOLDER#789       FILE#abc          Arquivo na pasta
FOLDER#789       FILE#def          Outro arquivo
```

**GSI3: Queries por Company**
```yaml
- IndexName: GSI3-Index
  KeySchema:
    - AttributeName: GSI3PK
      KeyType: HASH
```

**Uso**: Listar todos os recursos de uma empresa
```
GSI3PK           Descrição
--------------------------------
COMPANY#XYZ      Todos os tenants da empresa
```

---

## ?? Arquitetura em Camadas e o Template

### **Como o SAM Compila as Camadas**

1. **SAM CLI** executa `sam build`
2. Lê o `CodeUri: ./src/Arda9Tenant.Api/`
3. Executa `dotnet publish` no projeto **Arda9Tenant.Api**
4. O MSBuild resolve **todas as dependências**:
   ```
   Arda9Tenant.Api
   ??? Arda9Tenant.Application (via ProjectReference)
   ??? Arda9Tenant.Core (via ProjectReference)
   ??? Arda9Tenant.Domain (via ProjectReference)
   ??? Arda9Tenant.Infra (via ProjectReference)
   ```
5. Todas as DLLs são copiadas para `.aws-sam/build/NetCodeWebAPIServerless/`
6. O pacote é criado e enviado para S3 durante o deploy

### **? Você NÃO precisa**:
- ? Adicionar múltiplos `CodeUri` (um por camada)
- ? Criar múltiplas funções Lambda
- ? Configurar builds separados
- ? Copiar DLLs manualmente

**O SAM faz tudo automaticamente!** ?

---

## ?? Outputs

```yaml
Outputs:
  WebEndpoint:
    Description: "API Gateway endpoint URL"
    Value: !Sub "https://${ServerlessHttpApi}.execute-api.${AWS::Region}.amazonaws.com/"
```

**Após o deploy**:
```bash
sam deploy

# Output:
# WebEndpoint: https://abc123def.execute-api.us-east-1.amazonaws.com/
```

**Uso**:
```bash
# Testar endpoint
curl https://abc123def.execute-api.us-east-1.amazonaws.com/api/tenants
```

---

## ?? Comandos SAM

### **Validar Template**
```bash
sam validate
```

### **Build**
```bash
sam build
```

### **Local API**
```bash
sam local start-api
```

### **Deploy (Primeira vez)**
```bash
sam deploy --guided
```

### **Deploy (Subsequente)**
```bash
sam deploy
```

### **Delete Stack**
```bash
sam delete --stack-name arda9-tenant-api
```

---

## ?? Configurações Recomendadas

### **Para Produção**

```yaml
Globals:
  Function:
    Timeout: 60
    MemorySize: 1024
    Environment:
      Variables:
        ASPNETCORE_ENVIRONMENT: Production
        LOG_LEVEL: Warning
    Tracing: Active  # AWS X-Ray
```

### **Para Staging**

```yaml
Globals:
  Function:
    Timeout: 60
    MemorySize: 512
    Environment:
      Variables:
        ASPNETCORE_ENVIRONMENT: Staging
        LOG_LEVEL: Information
```

### **Para Development**

```yaml
Globals:
  Function:
    Timeout: 100
    MemorySize: 1024
    Environment:
      Variables:
        ASPNETCORE_ENVIRONMENT: Development
        LOG_LEVEL: Debug
```

---

## ?? Segurança

### **IAM Roles (Automático)**

SAM cria automaticamente uma IAM Role com:
- Permissões básicas de Lambda
- Políticas definidas em `Policies`
- CloudWatch Logs

### **Recomendações Adicionais**

```yaml
# VPC Configuration (se necessário)
VpcConfig:
  SecurityGroupIds:
    - sg-12345678
  SubnetIds:
    - subnet-12345678
    - subnet-87654321

# Dead Letter Queue
DeadLetterQueue:
  Type: SQS
  TargetArn: !GetAtt DeadLetterQueue.Arn

# Reserved Concurrent Executions
ReservedConcurrentExecutions: 10
```

---

## ?? Monitoramento

### **CloudWatch Logs**

Automaticamente criado em:
```
/aws/lambda/arda9-tenant-api-NetCodeWebAPIServerless-XXXXX
```

### **CloudWatch Metrics**

- Invocations
- Duration
- Errors
- Throttles
- ConcurrentExecutions

### **AWS X-Ray (Opcional)**

```yaml
Tracing: Active
```

---

## ? Checklist de Validação

Antes de fazer deploy:

- [x] `Handler` corresponde ao `AssemblyName` no `.csproj`
- [x] `Runtime` corresponde ao `TargetFramework`
- [x] `CodeUri` aponta para o projeto correto
- [x] Todas as camadas são referenciadas via `ProjectReference`
- [x] `CopyLocalLockFileAssemblies=true` no `.csproj`
- [x] `sam validate` executa sem erros
- [x] `sam build` executa sem erros
- [x] `sam local start-api` funciona

---

## ?? Referências

- [AWS SAM Specification](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/sam-specification.html)
- [AWS Lambda .NET](https://docs.aws.amazon.com/lambda/latest/dg/lambda-csharp.html)
- [DynamoDB Single Table Design](https://www.alexdebrie.com/posts/dynamodb-single-table/)

---

**Última Atualização**: 2024  
**Versão do SAM**: 2016-10-31  
**.NET**: 8.0
