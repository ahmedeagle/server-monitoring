# 🚀 Quick Start - One Command!

## Single Script Approach

**We've consolidated everything into ONE script for simplicity:**

```powershell
# Ensure Docker Desktop is running, then:
.\test-local.ps1
```

> **Note:** All other scripts (start-backend.ps1, start-frontend.ps1, etc.) are for development only. Use `test-local.ps1` for testing and deployment.

**The script will:**
- ✅ Build all services (SQL Server, Redis, API, Web)
- ✅ Wait for health checks
- ✅ Test endpoints
- ✅ Display access URLs

## Access Application

| Service | URL | Credentials |
|---------|-----|-------------|
| **Frontend** | http://localhost:3000 | admin / Admin123! |
| **API/Swagger** | http://localhost:5000/swagger | - |
| **Hangfire Jobs** | http://localhost:5000/hangfire | - |

## Verify Features

1. **Login** - Use admin / Admin123!
2. **Dashboard** - See real-time metrics (updates every 30s via SignalR)
3. **Servers** - Add/edit/delete servers (CRUD operations)
4. **Alerts** - View system alerts
5. **Reports** - Generate PDF/Excel reports
6. **Background Jobs** - Check Hangfire dashboard

## Cleanup

```powershell
docker compose down -v
```

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Port already in use | Run: `docker compose down -v` |
| Services not healthy | Wait 2 minutes for initialization |
| Docker not running | Start Docker Desktop |

**Need more help?** See [docs/LOCAL_TESTING_GUIDE.md](docs/LOCAL_TESTING_GUIDE.md)

## What's Implemented

- ✅ 8 Database entities with relationships
- ✅ Clean Architecture (Domain → Application → Infrastructure → Presentation)
- ✅ SOLID principles throughout
- ✅ JWT authentication + refresh tokens
- ✅ SignalR real-time updates
- ✅ Hangfire background jobs (4 types)
- ✅ Windows PerformanceCounter monitoring (CPU, Memory, Disk, Network)
- ✅ React 18 + TypeScript frontend
- ✅ 95 tests passing (65% coverage)
- ✅ Docker + AWS ECS ready

**For architecture details:** See [ARCHITECTURE.md](ARCHITECTURE.md)

**For deployment options:** See [docs/](docs/)
