# 🔒 Election API - Security Guide
**Version 1.1.1 - Hybrid Photo System + Debug Tools**

## 🚨 Important Security Notice

Este projeto contém arquivos de exemplo seguros para Docker e configuração. **NUNCA** commite dados sensíveis para o GitHub.

## 📋 Checklist de Segurança

### ✅ Antes do Deploy em Produção:

1. **Variáveis de Ambiente**:
   - [ ] Copie `.env.example` para `.env`
   - [ ] Substitua TODAS as senhas padrão
   - [ ] Use chaves JWT de 32+ caracteres
   - [ ] Configure SMTP real para emails

2. **Senhas Obrigatórias**:
   - [ ] `DB_ROOT_PASSWORD` - Senha MySQL root
   - [ ] `DB_PASSWORD` - Senha usuário banco
   - [ ] `JWT_SECRET_KEY` - Chave JWT (32+ chars)
   - [ ] `VOTE_MASTER_KEY` - Chave criptografia votos
   - [ ] `VOTE_JUSTIFICATION_KEY` - Chave justificativas
   - [ ] `SWAGGER_PASSWORD` - Senha Swagger UI

3. **Configuração SMTP**:
   - [ ] `SMTP_USERNAME` - Email válido
   - [ ] `SMTP_PASSWORD` - App password ou senha
   - [ ] `SMTP_FROM_EMAIL` - Email remetente

### 🔐 Geração de Chaves Seguras

```bash
# Gerar chave JWT segura (32+ caracteres)
openssl rand -base64 32

# Gerar chaves de criptografia de votos
openssl rand -base64 48
```

### 🚀 Deploy Seguro com Docker

```bash
# 1. Configure as variáveis de ambiente
cp .env.example .env
nano .env  # Configure todas as variáveis

# 2. Build com versão
docker-compose build --build-arg APP_VERSION=1.1.0

# 3. Deploy
docker-compose up -d
```

## 📁 Arquivos de Segurança

| Arquivo | Status | Descrição |
|---------|--------|-----------|
| `.env.example` | ✅ Seguro | Template com valores de exemplo |
| `.env` | ❌ Sensível | Valores reais - NUNCA commite |
| `docker-compose.yml` | ✅ Seguro | Usa variáveis de ambiente |
| `appsettings.json` | ✅ Seguro | Sem senhas hardcoded |

## 🛡️ Recursos de Segurança v1.1.1

### Sistema Híbrido de Fotos:
- **Otimização ImageSharp**: Redimensionamento automático
- **Compressão inteligente**: JPEG 85% qualidade
- **Validação de arquivos**: Tipos permitidos controlados
- **Auditoria completa**: Logs de acesso a fotos
- **Armazenamento dual**: BLOB + arquivo (compatibilidade)

### Criptografia de Votos:
- **Chaves separadas**: Master key + Justification key
- **AES-256**: Criptografia forte para dados sensíveis
- **Salt único**: Para cada voto registrado

### Autenticação JWT:
- **Tokens temporários**: Admin (60min) / Voter (10min)
- **Roles separadas**: Controle de acesso granular
- **Refresh automático**: Via frontend

## ⚠️ Alertas de Segurança

### ❌ NUNCA faça:
- Commitar arquivos `.env` com dados reais
- Usar senhas padrão em produção
- Expor Swagger em produção sem autenticação
- Reutilizar chaves entre ambientes

### ✅ SEMPRE faça:
- Use HTTPS em produção
- Configure firewall no servidor
- Monitore logs de auditoria
- Faça backup das chaves de criptografia
- Teste deployment em ambiente staging primeiro

## 🔍 Monitoramento

### Logs de Auditoria:
```bash
# Verificar logs da aplicação
docker-compose logs api

# Verificar logs específicos de foto
grep "photo" logs/election-api-*.txt
```

### Health Checks:
- **API**: `GET /health`
- **Database**: Verificação automática no docker-compose
- **Swagger**: `GET /swagger` (apenas dev/staging)

## 📞 Suporte

Em caso de problemas de segurança:
1. **NÃO** exponha dados sensíveis em issues públicas
2. Configure novas chaves se houver suspeita de comprometimento
3. Revise logs de auditoria regularmente
4. Mantenha backups seguros das configurações

---
**🤖 Generated with [Claude Code](https://claude.ai/code)**