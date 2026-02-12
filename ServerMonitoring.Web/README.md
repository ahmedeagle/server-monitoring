# Server Monitoring Frontend

React + TypeScript + Material-UI frontend for the Server Monitoring Dashboard.

## Features

- 🔐 JWT Authentication
- 📊 Real-time Dashboard with live charts
- 📡 SignalR integration for live updates
- 🖥️ Server management (CRUD operations)
- ⚠️ Alert monitoring and management
- 📄 Report generation
- 🎨 Material-UI components
- 🌓 Dark/Light theme toggle
- 📱 Responsive design

## Tech Stack

- React 18
- TypeScript
- Vite
- Material-UI (MUI)
- Recharts
- SignalR Client
- Zustand (State Management)
- Axios
- React Router v6

## Prerequisites

- Node.js 18+ and npm
- Backend API running on http://localhost:5000

## Installation

```bash
cd ServerMonitoring.Web
npm install
```

## Development

```bash
npm run dev
```

Access the application at http://localhost:3000

## Build

```bash
npm run build
```

## Test Credentials

- **Admin**: username=`admin`, password=`Admin@123`
- **User**: username=`user`, password=`User@123`

## API Configuration

The Vite dev server proxies API requests to the backend:
- `/api/*` → http://localhost:5000/api
- `/hubs/*` → http://localhost:5000/hubs (SignalR WebSocket)

## Project Structure

```
src/
├── components/       # Reusable components
│   └── Layout.tsx   # Main layout with sidebar & topbar
├── contexts/        # React contexts
│   └── SignalRContext.tsx
├── pages/           # Page components
│   ├── LoginPage.tsx
│   ├── DashboardPage.tsx
│   ├── ServersPage.tsx
│   ├── ServerDetailsPage.tsx
│   ├── AlertsPage.tsx
│   ├── ReportsPage.tsx
│   ├── JobsPage.tsx
│   └── UsersPage.tsx
├── routes/          # Route configuration
├── services/        # API services
│   ├── api.ts
│   ├── authService.ts
│   ├── serverService.ts
│   ├── alertService.ts
│   └── reportService.ts
├── store/           # State management
│   └── authStore.ts
├── App.tsx
└── main.tsx
```

## Features Implementation

### Authentication
- Login page with form validation
- JWT token storage in localStorage
- Axios interceptors for automatic token injection
- Auto-redirect to login on 401 responses

### Real-Time Dashboard
- Live metric charts (CPU, Memory, Disk)
- Server selection
- Alert notifications
- SignalR connection status indicator

### Server Management
- List all servers
- Add/Edit/Delete servers (Admin only)
- View server details with historical metrics
- Multiple chart types (Line, Area)

### Alert Management
- List all alerts
- Filter by unacknowledged
- Acknowledge alerts
- Resolve alerts
- Color-coded severity (Critical, Warning, Info)

### Report Generation
- Generate reports with custom date range
- Multiple report types
- Track report status (Pending, Processing, Completed)
- Download completed reports

### Background Jobs
- Link to Hangfire dashboard
- Job descriptions

## State Management

Uses Zustand for lightweight state management:
- Authentication state (token, user, login/logout)
- Persisted to localStorage

## SignalR Integration

Automatic connection management:
- Connects on authentication
- Reconnects automatically
- Handles disconnections gracefully
- Listens for:
  - `ReceiveMetricUpdate` - Real-time metrics
  - `ReceiveAlert` - New alerts

## Responsive Design

- Mobile-first approach
- Responsive sidebar (drawer)
- Adaptive charts
- Touch-friendly controls
