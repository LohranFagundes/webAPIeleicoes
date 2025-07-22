# Setup da Election API .NET

## Configuração Completada ✅

A API foi completamente convertida do PHP para .NET 8 C# e está configurada com:

### 🔐 Configuração de Ambiente (.env)
A aplicação agora usa arquivos `.env` para gerenciar dados sensíveis de forma segura.

### Credenciais do Banco de Dados (via .env)
- **Host**: localhost
- **Porta**: 3306  
- **Usuário**: root
- **Senha**: super-secret-password (configurado no .env)
- **Banco**: election_system

### Migrações do Banco de Dados
- ✅ Migrações criadas e executadas
- ✅ Todas as tabelas criadas no MySQL
- ✅ Dados de exemplo inseridos

### Estrutura das Tabelas Criadas
- `admins` - Contas de administradores
- `voters` - Contas de eleitores  
- `elections` - Definições de eleições
- `positions` - Cargos/posições nas eleições
- `candidates` - Candidatos para os cargos
- `votes` - Votos registrados
- `audit_logs` - Log de auditoria do sistema

## Como Executar

### ⚙️ Configuração Inicial

1. **Configure as variáveis de ambiente:**
   ```bash
   # Copie o arquivo de exemplo
   cp .env.example .env
   
   # Edite o arquivo .env com suas credenciais
   nano .env  # ou use seu editor preferido
   ```

2. **Executar a API:**
   ```bash
   cd ElectionApi.Net
   dotnet run --urls "http://localhost:5000"
   ```

2. **Acessar a documentação:**
   - Swagger UI: http://localhost:5185/swagger (ou a porta que aparecer no console)
   - API Info: http://localhost:5185/
   - Health Check: http://localhost:5185/health
   - Redirect: http://localhost:5185/docs (redireciona para /swagger)

## Dados de Teste Criados

### Admin Padrão
- **Email**: admin@election-system.com
- **Senha**: admin123
- **Permissões**: Administrador completo

### Eleitor de Exemplo  
- **Email**: joao@example.com
- **Senha**: voter123
- **Status**: Ativo e verificado

### Eleição de Exemplo
- **Título**: "Eleição de Exemplo 2024"
- **Status**: Draft
- **Candidatos**: Maria Santos (10) e Carlos Oliveira (20)

## Endpoints Principais

### Autenticação
- `POST /api/auth/admin/login` - Login de administrador
- `POST /api/auth/voter/login` - Login de eleitor
- `POST /api/auth/logout` - Logout
- `POST /api/auth/validate` - Validar token

### Eleições (Requer autenticação de Admin)
- `GET /api/election` - Listar eleições
- `GET /api/election/{id}` - Detalhes da eleição
- `POST /api/election` - Criar eleição
- `PUT /api/election/{id}` - Atualizar eleição
- `DELETE /api/election/{id}` - Deletar eleição
- `PATCH /api/election/{id}/status` - Atualizar status

### Público
- `GET /api/election/active` - Eleições ativas

## Exemplo de Login (Admin)

```bash
curl -X POST http://localhost:5185/api/auth/admin/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@election-system.com",
    "password": "admin123"
  }'
```

## Exemplo de Login (Eleitor)

```bash
curl -X POST http://localhost:5185/api/auth/voter/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "joao@example.com",
    "password": "voter123"
  }'
```

## Recursos Implementados

- ✅ Autenticação JWT
- ✅ Autorização baseada em roles
- ✅ Criptografia de senhas com BCrypt
- ✅ Logging estruturado com Serilog
- ✅ Middleware de CORS, Exception Handling e Logging
- ✅ Documentação Swagger/OpenAPI
- ✅ Validação de entrada com FluentValidation
- ✅ Repository Pattern
- ✅ Sistema de auditoria
- ✅ Dados de exemplo para testes

## Arquitetura

```
ElectionApi.Net/
├── Controllers/     # Endpoints da API
├── Models/          # Entidades do banco
├── Services/        # Lógica de negócio  
├── Data/            # Acesso a dados
├── DTOs/            # Transfer Objects
├── Middleware/      # Middlewares personalizados
└── Migrations/      # Migrações do EF Core
```

## 🔐 Configuração de Variáveis de Ambiente

### Arquivo .env

O projeto usa um arquivo `.env` para gerenciar dados sensíveis:

```bash
# Database Configuration
DB_HOST=localhost
DB_PORT=3306
DB_DATABASE=election_system
DB_USERNAME=root
DB_PASSWORD=sua_senha_mysql

# JWT Configuration  
JWT_SECRET_KEY=sua-chave-secreta-de-pelo-menos-32-caracteres
JWT_ISSUER=ElectionApi
JWT_AUDIENCE=ElectionApiUsers
JWT_EXPIRE_MINUTES=60

# Application Configuration
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5000
```

### Segurança

- ✅ Arquivo `.env` está no `.gitignore` (não será commitado)
- ✅ Existe `.env.example` como template público
- ✅ Fallback para valores padrão se .env não existir
- ✅ Separação entre configuração de desenvolvimento e produção

## 📋 Preparação para GitHub

### Arquivos Criados para Versionamento:
- ✅ `.gitignore` - Ignora arquivos sensíveis e de build
- ✅ `.env.example` - Template de configuração
- ✅ `SETUP.md` - Instruções de configuração
- ✅ `README.md` - Documentação principal

### Antes do Primeiro Commit:
1. Certifique-se que o arquivo `.env` não será commitado
2. Verifique se `.env.example` tem todas as variáveis necessárias
3. Teste a aplicação localmente

A API está pronta para uso e versionamento no GitHub! 🚀