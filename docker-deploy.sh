#!/bin/bash

# =============================================================================
# SCRIPT DE DEPLOY DOCKER - SISTEMA DE ELEIÇÕES COM FOTOS HÍBRIDAS
# =============================================================================

set -e  # Exit on any error

echo "🚀 === INICIANDO DEPLOY DO SISTEMA DE ELEIÇÕES ==="
echo "📸 Sistema Híbrido de Fotos (BLOB + Arquivo)"
echo "🐳 Docker + MySQL + .NET 8.0"
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if Docker is running
print_status "Verificando se Docker está rodando..."
if ! docker info > /dev/null 2>&1; then
    print_error "Docker não está rodando. Por favor, inicie o Docker Desktop."
    exit 1
fi
print_success "Docker está rodando ✓"

# Stop existing containers
print_status "Parando containers existentes..."
docker-compose down --remove-orphans || true
print_success "Containers parados ✓"

# Remove old images to ensure fresh build
print_status "Removendo imagens antigas para garantir build limpo..."
docker image prune -f || true
docker rmi electionapinet-api:latest 2>/dev/null || true
print_success "Limpeza concluída ✓"

# Build new images
print_status "Construindo nova imagem com sistema híbrido de fotos..."
docker-compose build --no-cache --parallel
print_success "Build concluído ✓"

# Start services
print_status "Iniciando serviços (MySQL + API)..."
docker-compose up -d
print_success "Serviços iniciados ✓"

# Wait for services to be healthy
print_status "Aguardando serviços ficarem saudáveis..."
echo "⏳ Aguardando MySQL..."
sleep 5

# Check MySQL health
for i in {1..30}; do
    if docker-compose exec -T db mysqladmin ping -h localhost -u root -pElectionSystem2024! --silent; then
        print_success "MySQL está saudável ✓"
        break
    fi
    if [ $i -eq 30 ]; then
        print_error "MySQL não respondeu em 30 tentativas"
        docker-compose logs db
        exit 1
    fi
    echo "  Tentativa $i/30..."
    sleep 2
done

echo "⏳ Aguardando API..."
sleep 10

# Check API health
for i in {1..20}; do
    if curl -s -f http://localhost:5110/health > /dev/null 2>&1; then
        print_success "API está saudável ✓"
        break
    fi
    if [ $i -eq 20 ]; then
        print_warning "API não respondeu no endpoint /health, mas pode estar funcionando"
        break
    fi
    echo "  Tentativa $i/20..."
    sleep 3
done

# Show running containers
print_status "Containers em execução:"
docker-compose ps

# Show logs
print_status "Últimos logs da API:"
docker-compose logs --tail=20 api

echo ""
echo "🎉 === DEPLOY CONCLUÍDO COM SUCESSO ==="
echo ""
echo "📡 ENDPOINTS DISPONÍVEIS:"
echo "   • API Base: http://localhost:5110"
echo "   • Swagger: http://localhost:5110/swagger"
echo "   • Health Check: http://localhost:5110/health"
echo ""
echo "📸 NOVOS ENDPOINTS DE FOTOS HÍBRIDAS:"
echo "   • Upload BLOB: POST /api/candidate/{id}/upload-photo-blob"
echo "   • Get Photo Smart: GET /api/candidate/{id}/photo"
echo "   • Upload File: POST /api/candidate/{id}/upload-photo"
echo ""
echo "🗄️ BANCO DE DADOS:"
echo "   • Host: localhost:3306"
echo "   • Database: election_system"
echo "   • User: root"
echo "   • Password: ElectionSystem2024!"
echo ""
echo "🔧 COMANDOS ÚTEIS:"
echo "   • Ver logs: docker-compose logs -f api"
echo "   • Parar: docker-compose down"
echo "   • Rebuild: ./docker-deploy.sh"
echo "   • Entrar no container: docker-compose exec api bash"
echo ""
print_success "Sistema pronto para uso! 🚀"