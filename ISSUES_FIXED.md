# 🔧 Issues Fixed & Solutions

## Issue 1: Dashboard Shows "Disconnected"

**Root Cause**: SignalR connection works, but Web container doesn't have correct API_URL environment variable set.

**What was the problem:**
- The Web task definition was created without the API_URL environment variable
- SignalR hub URL uses relative path `/hubs/monitoring` which gets proxied through nginx
- Nginx config HAS the `/hubs` proxy configured correctly ✅
- But the API_HOST variable in nginx config may be wrong

**Current Status**: 
- ✅ nginx config at lines 82-96 has `/hubs` proxy configured
- ❌ Need to verify API_URL in Web task definition points to ALB

**Fix**: Web task definition should have:
```json
"environment": [
  {
    "name": "API_URL",
    "value": "http://servermonitoring-alb-1100072309.us-east-1.elb.amazonaws.com/api"
  }
]
```

---

## Issue 2: Hangfire Dashboard Not Accessible

**Endpoint**: `/hangfire`

**Root Cause**: Hangfire dashboard is configured in API (Program.cs line 276), but nginx doesn't have a proxy rule for it.

**Fix Required**: Add nginx location block for `/hangfire`:

```nginx
# Hangfire Dashboard proxy
location /hangfire {
    proxy_pass http://api_backend;
    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection 'upgrade';
    proxy_set_header Host $host;
    proxy_cache_bypass $http_upgrade;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
    proxy_read_timeout 300;
}
```

---

## Issue 3: Download Report Returns 404

**Attempted URL**: `/api/v1/reports/1/download`

**Root Cause**: **The download endpoint doesn't exist in ReportsController.cs!**

**Current Available Endpoints**:
- ✅ GET `/api/v1/reports` - Get all reports
- ✅ POST `/api/v1/reports/generate?serverId=X&days=Y` - Generate report
- ✅ POST `/api/v1/reports/schedule?serverId=X&delayMinutes=Y` - Schedule report
- ✅ POST `/api/v1/reports/collect-metrics` - Trigger metrics collection
- ✅ GET `/api/v1/reports/{reportId}/status` - Get report status
- ❌ **MISSING**: GET `/api/v1/reports/{reportId}/download` - Download report

**Fix Required**: Implement download endpoint in ReportsController.cs

---

## Issue 4: Generated Report Not Listed

**What Works**:
- ✅ POST `/api/v1/reports/generate` returns 200 with reportId
- ✅ Report queued in Hangfire

**What Doesn't Work**:
- ❌ Report not appearing in GET `/api/v1/reports` list
- ❌ Report not being processed by Hangfire workers

**Root Causes**:
1. **API endpoint mismatch**: Current endpoint expects query params (`serverId`, `days`), but frontend sends JSON body with different structure
2. **Hangfire workers**: May not be running or configured correctly
3. **Report persistence**: ReportGenerationJob may not be saving reports to database

**Current Generate Endpoint Signature**:
```csharp
[HttpPost("generate")]
public IActionResult GenerateReport([FromQuery] int serverId, [FromQuery] int days = 7)
```

**Frontend Sends**:
```json
{
  "title": "daily test",
  "description": "",
  "type": "DailyMetrics",
  "startDate": "2026-02-06",
  "endDate": "2026-02-13"
}
```

**Mismatch!** The frontend expects a different API contract.

---

## Issue 5: How to See Running Jobs

**Solution**: Access Hangfire Dashboard after fixing Issue #2:
- URL: `http://servermonitoring-alb-1100072309.us-east-1.elb.amazonaws.com/hangfire`
- This shows:
  - ✅ Enqueued jobs
  - ✅ Processing jobs
  - ✅ Succeeded jobs
  - ✅ Failed jobs
  - ✅ Scheduled jobs
  - ✅ Recurring jobs

---

## Priority Fixes

### 🔴 CRITICAL (Fix Now):
1. Add `/hangfire` proxy to nginx config
2. Check Web task definition API_URL environment variable
3. Rebuild and redeploy Web image

### 🟡 MEDIUM (Fix Soon):
4. Implement download endpoint in ReportsController
5. Fix generate endpoint to match frontend contract OR fix frontend to match backend

### 🟢 LOW (Nice to Have):
6. Add proper report persistence in ReportGenerationJob
7. Verify Hangfire workers are processing jobs

