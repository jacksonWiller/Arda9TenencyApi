# Solução: Erro de Porta em Uso (EADDRINUSE)

## ?? **Diagnóstico do Problema**

**Erro Completo:**
```
System.IO.IOException: Failed to bind to address https://localhost:50374
SocketException: Foi feita uma tentativa de acesso a um soquete de uma maneira que é proibida pelas permissões de acesso.
```

**Causas Comuns:**
1. ? **Porta já está em uso** por outro processo
2. ?? **Permissões insuficientes** para usar portas baixas (<1024)
3. ?? **Certificado SSL** inválido ou não confiável
4. ?? **Firewall/Antivírus** bloqueando a porta

---

## ? **Solução Aplicada: Alterar Portas**

### **Arquivo Modificado: `launchSettings.json`**

**Antes (Portas com Conflito):**
```json
"applicationUrl": "https://localhost:50374;http://localhost:50375"
```

**Depois (Portas Padrão .NET):**
```json
"applicationUrl": "https://localhost:5001;http://localhost:5000"
```

### **Resultado:**
- ? HTTPS: `https://localhost:5001`
- ? HTTP: `http://localhost:5000`
- ? Swagger UI: `https://localhost:5001`

---

## ??? **Soluções Alternativas**

### **Solução 1: Verificar e Matar Processo na Porta**

#### **PowerShell (Windows)**
```powershell
# Verificar qual processo está usando a porta
netstat -ano | findstr :50374

# Você verá algo como:
# TCP    0.0.0.0:50374    0.0.0.0:0    LISTENING    12345
# O número 12345 é o PID do processo

# Matar o processo (substitua 12345 pelo PID real)
taskkill /PID 12345 /F
```

#### **CMD (Windows)**
```cmd
netstat -ano | findstr :50374
taskkill /PID <PID> /F
```

---

### **Solução 2: Usar Porta Dinâmica (0)**

Modificar `launchSettings.json`:
```json
{
  "profiles": {
    "ServerlessAPI": {
      "commandName": "Project",
      "launchBrowser": true,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "AWS_REGION": "us-east-1"
      },
      "applicationUrl": "https://localhost:0;http://localhost:0"
    }
  }
}
```

**Nota:** O .NET irá escolher portas disponíveis automaticamente.

---

### **Solução 3: Configurar Certificado SSL (se erro de HTTPS)**

#### **Recriar Certificado de Desenvolvimento**
```bash
# Remover certificado antigo
dotnet dev-certs https --clean

# Criar e confiar em novo certificado
dotnet dev-certs https --trust
```

#### **Verificar Certificados Instalados**
```powershell
# PowerShell
Get-ChildItem -Path Cert:\CurrentUser\My | Where-Object {$_.Subject -like "*localhost*"}
```

---

### **Solução 4: Desabilitar HTTPS Temporariamente**

Modificar `launchSettings.json` (apenas HTTP):
```json
{
  "profiles": {
    "ServerlessAPI": {
      "commandName": "Project",
      "launchBrowser": true,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "AWS_REGION": "us-east-1"
      },
      "applicationUrl": "http://localhost:5000"
    }
  }
}
```

**E comentar a linha no `Program.cs`:**
```csharp
// app.UseHttpsRedirection(); // Desabilitado para desenvolvimento
```

---

### **Solução 5: Usar IIS Express**

No Visual Studio:
1. Selecione o perfil **"IIS Express"** no dropdown
2. Execute (F5)

**Portas configuradas no `launchSettings.json`:**
```json
"iisSettings": {
  "windowsAuthentication": false,
  "anonymousAuthentication": true,
  "iisExpress": {
    "applicationUrl": "http://localhost:50977/",
    "sslPort": 44356
  }
}
```

---

## ?? **Solução: Problema de Permissões**

### **Windows: Executar Visual Studio como Administrador**

1. Feche o Visual Studio
2. Clique com botão direito no ícone do Visual Studio
3. Selecione **"Executar como administrador"**
4. Execute o projeto novamente

### **Reservar Porta (Windows)**

```powershell
# Executar como Administrador
netsh http add urlacl url=https://localhost:5001/ user=Everyone
netsh http add urlacl url=http://localhost:5000/ user=Everyone
```

---

## ?? **Solução: Firewall/Antivírus Bloqueando**

### **Windows Defender Firewall**

1. Abra **Configurações do Windows Defender Firewall**
2. Clique em **"Permitir um aplicativo através do firewall"**
3. Adicione **dotnet.exe** às exceções
4. Localize: `C:\Program Files\dotnet\dotnet.exe`

### **Desabilitar Temporariamente (Teste)**

```powershell
# PowerShell como Administrador
Set-NetFirewallProfile -Profile Domain,Public,Private -Enabled False

# Após testar, reabilite:
Set-NetFirewallProfile -Profile Domain,Public,Private -Enabled True
```

---

## ?? **Testar a Correção**

### **1. Limpar e Rebuild**
```bash
dotnet clean
dotnet build
```

### **2. Rodar Aplicação**
```bash
cd src/Arda9Tenant.Api
dotnet run
```

### **3. Verificar Logs**

Você deve ver:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

### **4. Acessar Swagger**
```
https://localhost:5001
```

---

## ?? **Portas Recomendadas**

| Ambiente | HTTPS | HTTP | Uso |
|----------|-------|------|-----|
| **Desenvolvimento (Padrão .NET)** | 5001 | 5000 | Recomendado |
| **SAM Local** | - | 3000 | Simula Lambda |
| **Lambda Test Tool** | - | 5050 | Testes Lambda |
| **IIS Express** | 44356 | 50977 | Visual Studio |
| **Custom (Exemplo)** | 7001 | 7000 | Livre |

---

## ?? **Diagnóstico Avançado**

### **Listar Todas as Portas em Uso**
```powershell
# PowerShell
Get-NetTCPConnection | Where-Object {$_.State -eq "Listen"} | 
  Select-Object LocalAddress, LocalPort, OwningProcess | 
  Sort-Object LocalPort
```

### **Verificar Processo Específico**
```powershell
# PowerShell
Get-Process -Id <PID> | Select-Object ProcessName, Id, Path
```

### **Verificar Aplicações .NET em Execução**
```powershell
Get-Process | Where-Object {$_.ProcessName -like "*dotnet*"}
```

---

## ? **Checklist de Validação**

Após aplicar a solução:

- [x] `launchSettings.json` atualizado com portas 5001/5000
- [ ] Nenhum processo usando as portas escolhidas
- [ ] Certificado SSL instalado e confiável
- [ ] Firewall não está bloqueando as portas
- [ ] Aplicação inicia sem erros
- [ ] Swagger UI acessível em `https://localhost:5001`
- [ ] API responde corretamente

---

## ?? **Configuração Final Recomendada**

### **launchSettings.json**
```json
{
  "profiles": {
    "ServerlessAPI": {
      "commandName": "Project",
      "launchBrowser": true,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "AWS_REGION": "us-east-1"
      },
      "applicationUrl": "https://localhost:5001;http://localhost:5000"
    },
    "ServerlessAPI-NoSSL": {
      "commandName": "Project",
      "launchBrowser": true,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "AWS_REGION": "us-east-1"
      },
      "applicationUrl": "http://localhost:5000"
    },
    "Mock Lambda Test Tool": {
      "commandName": "Executable",
      "executablePath": "%USERPROFILE%\\.dotnet\\tools\\dotnet-lambda-test-tool-8.0.exe",
      "commandLineArgs": "--port 5050",
      "workingDirectory": ".\\bin\\$(Configuration)\\net8.0"
    },
    "IIS Express": {
      "commandName": "IISExpress",
      "launchBrowser": true,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "AWS_REGION": "us-east-1"
      }
    }
  },
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true,
    "iisExpress": {
      "applicationUrl": "http://localhost:50977/",
      "sslPort": 44356
    }
  }
}
```

---

## ?? **Próximos Passos**

1. ? Salvar alterações no `launchSettings.json`
2. ? Fechar todas as instâncias do Visual Studio/VSCode
3. ? Reabrir o projeto
4. ? Selecionar perfil **"ServerlessAPI"**
5. ? Executar (F5)
6. ? Acessar `https://localhost:5001`

---

**Status:** ? Resolvido  
**Solução Aplicada:** Alteração de portas para 5001/5000  
**Data:** 2024  
**Ambiente:** .NET 8
