## Índice
1. Autenticação
2. Modelos de Dados
3. Endpoints (Visão Geral)
4. Detalhamento de Endpoints
5. Tratamento de Erros
6. Rate Limiting
7. Webhooks

---

## 1. Autenticação

A API utiliza autenticação baseada em tokens JWT. O token deve ser enviado no cabeçalho `Authorization` de cada requisição protegida.

**Header:**
`Authorization: Bearer <access_token>`
`X-Tenant-ID: <tenant_id>` (Obrigatório para endpoints específicos de tenant)

---

## 2. Modelos de Dados

### Tenant
```typescript
export interface Tenant {
  id: number;
  name: string;
  domain: string;
  logo?: string;
  primaryColor: string;
  secondaryColor: string;
  clientsCount: number;
  usersCount: number;
  status: 'active' | 'inactive' | 'suspended';
  createdAt: string;
  plan: 'basic' | 'pro' | 'enterprise';
}
```

### Client
```typescript
export interface Client {
  id: number;
  tenantId: number;
  name: string;
  email: string;
  phone: string;
  company: string;
  status: 'active' | 'inactive' | 'pending';
  createdAt: string;
  lastContact?: string;
}
```

### User
```typescript
export interface User {
  id: number;
  tenantId: number;
  name: string;
  email: string;
  role: 'admin' | 'manager' | 'viewer';
  status: 'active' | 'pending' | 'inactive';
  invitedAt: string;
  lastLogin?: string;
  avatar?: string;
}
```

### ImportantData
```typescript
export interface ImportantData {
  id: number;
  tenantId: number;
  title: string;
  description: string;
  category: 'credentials' | 'documents' | 'certificates' | 'licenses' | 'contracts';
  priority: 'high' | 'medium' | 'low';
  expiryDate?: string;
  createdAt: string;
  updatedAt: string;
}
```

### ApiResponse
```typescript
export interface ApiResponse<T = any> {
  success: boolean;
  data?: T;
  error?: string;
  message?: string;
}
```

### PaginatedResponse
```typescript
export interface PaginatedResponse<T> {
  success: boolean;
  data: T[];
  total: number;
  page: number;
  pageSize: number;
  error?: string;
  message?: string;
}
```

---

## 3. Endpoints (Visão Geral)

### Auth
| Método | Endpoint | Descrição |
| :--- | :--- | :--- |
| POST | `/api/Auth/login` | Autentica um usuário e retorna tokens. |
| POST | `/api/Auth/register` | Registra um novo usuário/tenant. |
| GET | `/api/Auth/me` | Retorna informações do usuário logado. |
| POST | `/api/Auth/logout` | Invalida a sessão atual. |
| POST | `/api/Auth/refresh` | Renova o access token usando o refresh token. |
| POST | `/api/Auth/forgot-password` | Solicita recuperação de senha. |
| POST | `/api/Auth/reset-password` | Define uma nova senha com código de verificação. |

### Tenants
| Método | Endpoint | Descrição |
| :--- | :--- | :--- |
| GET | `/api/tenants` | Lista todos os tenants (suporta paginação e filtros). |
| GET | `/api/tenants/{id}` | Detalhes de um tenant específico. |
| POST | `/api/tenants` | Cria um novo tenant. |
| PATCH | `/api/tenants/{id}` | Atualiza dados de um tenant. |
| DELETE | `/api/tenants/{id}` | Remove um tenant. |
| POST | `/api/tenants/{id}/logo` | Upload de logo (Multipart/form-data). |

### Clients
| Método | Endpoint | Descrição |
| :--- | :--- | :--- |
| GET | `/api/tenants/{tenantId}/clients` | Lista clientes de um tenant específico. |
| GET | `/api/clients` | Lista global de clientes (Admin apenas). |
| POST | `/api/clients` | Cria um novo cliente. |
| PATCH | `/api/clients/{id}` | Atualiza um cliente. |
| DELETE | `/api/clients/{id}` | Remove um cliente. |
| POST | `/api/clients/bulk-import` | Importação em massa via CSV. |

### Users
| Método | Endpoint | Descrição |
| :--- | :--- | :--- |
| GET | `/api/tenants/{tenantId}/users` | Lista usuários de um tenant. |
| POST | `/api/users/invite` | Convida um novo usuário para o tenant. |
| PATCH | `/api/users/{id}` | Atualiza cargo ou status do usuário. |
| DELETE | `/api/users/{id}` | Remove acesso de um usuário. |
| POST | `/api/users/{id}/resend-invitation` | Reenvia e-mail de convite. |
| POST | `/api/users/{id}/avatar` | Upload de foto de perfil. |

### Important Data
| Método | Endpoint | Descrição |
| :--- | :--- | :--- |
| GET | `/api/tenants/{tenantId}/important-data` | Lista dados críticos do tenant. |
| GET | `/api/important-data/expiring` | Lista itens próximos do vencimento. |
| POST | `/api/important-data` | Adiciona novo dado crítico. |
| PATCH | `/api/important-data/{id}` | Atualiza dado crítico. |
| DELETE | `/api/important-data/{id}` | Remove dado crítico. |

### Analytics
| Método | Endpoint | Descrição |
| :--- | :--- | :--- |
| GET | `/api/analytics/dashboard` | Métricas globais da plataforma. |
| GET | `/api/analytics/tenants/{id}` | Crescimento e atividade de um tenant. |
| GET | `/api/analytics/platform-growth` | Tendências de crescimento da plataforma. |

### Search & Export
| Método | Endpoint | Descrição |
| :--- | :--- | :--- |
| GET | `/api/search` | Busca global em todas as entidades. |
| POST | `/api/tenants/{id}/export` | Inicia exportação assíncrona de dados. |
| GET | `/api/exports/{id}` | Status e link de download da exportação. |

---

## 4. Detalhamento de Endpoints

### Autenticação

#### POST `/api/Auth/login`
Autentica o usuário e retorna os tokens de acesso.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbG...",
    "idToken": "eyJhbG...",
    "refreshToken": "eyJhbG...",
    "expiresIn": 3600,
    "tokenType": "Bearer"
  }
}
```

#### POST `/api/Auth/register`
Cria uma nova conta de usuário e tenant.

**Request Body:**
```json
{
  "email": "newuser@example.com",
  "password": "securePassword123",
  "name": "John Doe",
  "phoneNumber": "+5511999999999"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "userSub": "uuid-v4-string",
    "userConfirmed": false,
    "message": "User registered successfully. Please verify your email."
  }
}
```

### Tenants

#### GET `/api/tenants`
Lista todos os tenants com suporte a filtros.

**Query Parameters:**
- `page` (int): Número da página (default: 1)
- `pageSize` (int): Itens por página (default: 10)
- `search` (string): Busca por nome ou domínio
- `status` (string): Filtro por status (active, inactive, suspended)

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Acme Corp",
      "domain": "acme.arda9.com",
      "status": "active",
      "clientsCount": 150,
      "usersCount": 12,
      "plan": "enterprise"
    }
  ],
  "total": 45,
  "page": 1,
  "pageSize": 10
}
```

#### POST `/api/tenants`
Cria um novo tenant.

**Request Body:**
```json
{
  "name": "New Tenant",
  "domain": "newtenant.arda9.com",
  "primaryColor": "#0066cc",
  "secondaryColor": "#4d94ff",
  "plan": "pro"
}
```

### Clients

#### POST `/api/clients/bulk-import`
Importação em massa de clientes via arquivo CSV.

**Request:**
- `Content-Type: multipart/form-data`
- `file`: Arquivo .csv contendo colunas `name, email, phone, company`.

**Response (200 OK):**
```json
{
  "success": true,
  "message": "150 clients imported successfully, 2 failed.",
  "data": {
    "imported": 150,
    "failed": 2,
    "errors": [
      { "line": 12, "reason": "Invalid email format" }
    ]
  }
}
```

### Users

#### POST `/api/users/invite`
Envia um convite por e-mail para um novo usuário.

**Request Body:**
```json
{
  "tenantId": 1,
  "email": "colleague@company.com",
  "role": "manager"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Invitation sent successfully."
}
```

### Important Data

#### GET `/api/important-data/expiring`
Retorna itens que estão próximos da data de vencimento.

**Query Parameters:**
- `days` (int): Janela de dias para vencimento (default: 30)
- `tenantId` (int): Filtrar por tenant específico

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": 45,
      "title": "SSL Certificate",
      "expiryDate": "2025-01-15",
      "priority": "high",
      "category": "certificates"
    }
  ]
}
```

---

## 5. Tratamento de Erros

A API utiliza códigos de status HTTP padrão:
- `200 OK`: Sucesso.
- `201 Created`: Recurso criado com sucesso.
- `400 Bad Request`: Erro de validação ou requisição inválida.
- `401 Unauthorized`: Token ausente ou inválido.
- `403 Forbidden`: Usuário sem permissão para o recurso.
- `404 Not Found`: Recurso não encontrado.
- `500 Internal Server Error`: Erro inesperado no servidor.

---

## 6. Rate Limiting

- **Autenticação**: 5 tentativas por minuto por IP.
- **API Geral**: 100 requisições por minuto por usuário.
- **Exportação**: 2 solicitações por hora por tenant.

---

## 7. Webhooks

Eventos disponíveis para integração:
- `tenant.created`: Disparado quando um novo tenant é registrado.
- `data.expiring`: Disparado 7 dias antes do vencimento de um item em "Important Data".
- `user.invited`: Disparado quando um convite é enviado.
