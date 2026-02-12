# =====================================================
# Docker Swarm Deployment Script
# Deploys Server Monitoring Application with HA
# =====================================================

param(
    [ValidateSet('init', 'deploy', 'scale', 'update', 'rollback', 'remove', 'status')]
    [string]$Action = 'status',
    
    [int]$ApiReplicas = 3,
    [int]$WebReplicas = 2,
    
    [string]$StackName = 'servermonitoring',
    [string]$Network = 'servermonitoring-network'
)

$ErrorActionPreference = 'Stop'

# Colors for output
function Write-ColorOutput($ForegroundColor) {
    $fc = $host.UI.RawUI.ForegroundColor
    $host.UI.RawUI.ForegroundColor = $ForegroundColor
    if ($args) {
        Write-Output $args
    }
    $host.UI.RawUI.ForegroundColor = $fc
}

function Write-Info($message) {
    Write-ColorOutput Cyan "ℹ️  $message"
}

function Write-Success($message) {
    Write-ColorOutput Green "✅ $message"
}

function Write-Warning($message) {
    Write-ColorOutput Yellow "⚠️  $message"
}

function Write-Error($message) {
    Write-ColorOutput Red "❌ $message"
}

function Write-Header($message) {
    Write-ColorOutput Magenta "`n========================================`n$message`n========================================"
}

# =====================================================
# Initialize Docker Swarm
# =====================================================
function Initialize-Swarm {
    Write-Header "Initializing Docker Swarm"
    
    $swarmStatus = docker info --format '{{.Swarm.LocalNodeState}}'
    
    if ($swarmStatus -eq 'active') {
        Write-Warning "Docker Swarm is already initialized"
        return
    }
    
    Write-Info "Initializing Docker Swarm..."
    docker swarm init
    
    Write-Success "Docker Swarm initialized successfully"
    Write-Info "Manager node IP: $(docker node inspect self --format '{{.Status.Addr}}')"
}

# =====================================================
# Create Docker Secrets
# =====================================================
function Create-Secrets {
    Write-Header "Creating Docker Secrets"
    
    $secrets = @{
        'sa_password' = 'YourStrong@Passw0rd123!'
        'redis_password' = 'YourRedis@Password123!'
        'jwt_secret' = 'YourSuperSecretKeyForJWT_MustBeAtLeast32Characters_ChangeInProduction!'
        'grafana_password' = 'admin@Grafana123!'
    }
    
    foreach ($secretName in $secrets.Keys) {
        $existingSecret = docker secret ls --filter name=$secretName --format '{{.Name}}'
        
        if ($existingSecret) {
            Write-Warning "Secret '$secretName' already exists, skipping..."
        }
        else {
            Write-Info "Creating secret: $secretName"
            $secrets[$secretName] | docker secret create $secretName -
            Write-Success "Created secret: $secretName"
        }
    }
}

# =====================================================
# Build Docker Images
# =====================================================
function Build-Images {
    Write-Header "Building Docker Images"
    
    Write-Info "Building API image..."
    docker build -t servermonitoring-api:latest -f src/Presentation/ServerMonitoring.API/Dockerfile .
    
    Write-Info "Building Web image..."
    docker build -t servermonitoring-web:latest -f ServerMonitoring.Web/Dockerfile ./ServerMonitoring.Web
    
    Write-Success "Docker images built successfully"
}

# =====================================================
# Deploy Stack
# =====================================================
function Deploy-Stack {
    Write-Header "Deploying Stack to Docker Swarm"
    
    Write-Info "Deploying stack: $StackName"
    docker stack deploy -c docker-compose.swarm.yml $StackName
    
    Write-Success "Stack deployed successfully"
    
    Write-Info "Waiting for services to stabilize..."
    Start-Sleep -Seconds 10
    
    Show-Status
}

# =====================================================
# Scale Services
# =====================================================
function Scale-Services {
    Write-Header "Scaling Services"
    
    Write-Info "Scaling API to $ApiReplicas replicas..."
    docker service scale "${StackName}_api=$ApiReplicas"
    
    Write-Info "Scaling Web to $WebReplicas replicas..."
    docker service scale "${StackName}_web=$WebReplicas"
    
    Write-Success "Services scaled successfully"
    
    Show-Status
}

# =====================================================
# Update Services
# =====================================================
function Update-Services {
    Write-Header "Updating Services"
    
    Build-Images
    
    Write-Info "Updating API service..."
    docker service update --image servermonitoring-api:latest "${StackName}_api"
    
    Write-Info "Updating Web service..."
    docker service update --image servermonitoring-web:latest "${StackName}_web"
    
    Write-Success "Services updated successfully"
}

# =====================================================
# Rollback Services
# =====================================================
function Rollback-Services {
    Write-Header "Rolling Back Services"
    
    Write-Warning "Rolling back API service to previous version..."
    docker service rollback "${StackName}_api"
    
    Write-Warning "Rolling back Web service to previous version..."
    docker service rollback "${StackName}_web"
    
    Write-Success "Services rolled back successfully"
}

# =====================================================
# Remove Stack
# =====================================================
function Remove-Stack {
    Write-Header "Removing Stack"
    
    Write-Warning "This will remove all services, networks, and configs (but not volumes)"
    $confirmation = Read-Host "Are you sure? (yes/no)"
    
    if ($confirmation -ne 'yes') {
        Write-Info "Operation cancelled"
        return
    }
    
    Write-Info "Removing stack: $StackName"
    docker stack rm $StackName
    
    Write-Success "Stack removed successfully"
}

# =====================================================
# Show Status
# =====================================================
function Show-Status {
    Write-Header "Docker Swarm Status"
    
    Write-Info "Swarm Nodes:"
    docker node ls
    
    Write-Info "`nStack Services:"
    docker stack services $StackName
    
    Write-Info "`nService Tasks:"
    docker service ls --filter name="${StackName}_"
    
    Write-Info "`nRunning Containers:"
    docker stack ps $StackName --no-trunc
    
    Write-Info "`nAPI Service Logs (last 20 lines):"
    docker service logs "${StackName}_api" --tail 20
}

# =====================================================
# Main Execution
# =====================================================
Write-Header "Docker Swarm Deployment Tool"

switch ($Action) {
    'init' {
        Initialize-Swarm
        Create-Secrets
    }
    'deploy' {
        Initialize-Swarm
        Create-Secrets
        Build-Images
        Deploy-Stack
    }
    'scale' {
        Scale-Services
    }
    'update' {
        Update-Services
    }
    'rollback' {
        Rollback-Services
    }
    'remove' {
        Remove-Stack
    }
    'status' {
        Show-Status
    }
}

Write-Info "`n🚀 Available Commands:"
Write-Info "  - Deploy:   .\deploy-swarm.ps1 -Action deploy"
Write-Info "  - Scale:    .\deploy-swarm.ps1 -Action scale -ApiReplicas 5 -WebReplicas 3"
Write-Info "  - Update:   .\deploy-swarm.ps1 -Action update"
Write-Info "  - Rollback: .\deploy-swarm.ps1 -Action rollback"
Write-Info "  - Status:   .\deploy-swarm.ps1 -Action status"
Write-Info "  - Remove:   .\deploy-swarm.ps1 -Action remove"
Write-Info "`n📊 Access Points:"
Write-Info "  - API:         http://localhost/api"
Write-Info "  - Web:         http://localhost"
Write-Info "  - Traefik:     http://localhost:8080"
Write-Info "  - Grafana:     http://localhost:3001"
Write-Info "  - Health:      http://localhost/health"
